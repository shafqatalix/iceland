
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

public class Proc
{
    private readonly CodeCompileUnit _compileUnit = new CodeCompileUnit();
    private CodeNamespace _codeNamespace;
    private readonly Procedure _procedure;
    private readonly CodeTypeReference voidType = new CodeTypeReference(typeof(void));
    private readonly string _outputFileName;
    private readonly string _databaseName;

    public Proc(string databaseName, Procedure procedure)
    {
        _procedure = procedure;
        _databaseName = databaseName;
        _outputFileName = @$"{procedure.Name}.cs";

    }
    private void AddImportStatements()
    {
        CodeNamespace globalNamespace = new CodeNamespace();
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System.Linq" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System.Data" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System.Collections.Generic" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "Microsoft.Data.SqlClient" });
        _compileUnit.Namespaces.Add(globalNamespace);
    }

    private void CreateNamespace()
    {
        _codeNamespace = new CodeNamespace(@$"{_databaseName}.Procedures.{_procedure.Schema}");
        _compileUnit.Namespaces.Add(_codeNamespace);
    }

    private CodeTypeDeclaration InputArgumentsClass()
    {
        if (_procedure.Parameters?.Length > 0)
        {
            var currentClass = Utility.GetClass(Utility.ParametersClassName(_procedure));
            foreach (var m in _procedure.Parameters)
            {
                CodeMemberField stringField = new CodeMemberField(Utility.MapType(m.Type), m.Name);
                stringField.Attributes = MemberAttributes.Public;
                currentClass.Members.Add(stringField);
            }
            _codeNamespace.Types.Add(currentClass);
            return currentClass;
        }
        return null;
    }

    private CodeTypeDeclaration ReturnTypeClass()
    {
        if (_procedure.ReturnType is null)
        {
            return null;
        }

        // Multi ResultSet
        if (_procedure.ReturnType.Length == 1 && _procedure.ReturnType[0].IsMultiResult == 1)
        {
            var multiClass = Utility.GetClass(Utility.ReturnTypeClassName(_procedure));
            for (var i = 0; i < _procedure.Dependencies.Length; i++)
            {
                var dep = Utility.ReturnTypeClassName(_procedure.Dependencies[i].ChildProcedure);
                var field = new CodeMemberField(dep, @$"resultSet{i}");
                field.Comments.Add(new CodeCommentStatement("<summary>"));
                field.Comments.Add(new CodeCommentStatement(@$"{dep} - Return fields"));
                field.Comments.Add(new CodeCommentStatement("<summary>"));
                field.Attributes = MemberAttributes.Public;
                multiClass.Members.Add(field);
            }
            _codeNamespace.Types.Add(multiClass);
            return multiClass;
        }

        var currentClass = Utility.GetClass(Utility.ReturnTypeClassName(_procedure));
        foreach (var m in _procedure.ReturnType.Where(t => !string.IsNullOrWhiteSpace(t.Name)))
        {
            CodeMemberField field = new CodeMemberField(Utility.MapType(m.Type), m.Name);
            field.Attributes = MemberAttributes.Public;
            currentClass.Members.Add(field);
        }
        _codeNamespace.Types.Add(currentClass);
        return currentClass;
    }

    private void AddParameters(CodeMemberMethod method)
    {
        var parametersExpr=new CodeSnippetStatement(@$"var parameters=new SqlParameter[{_procedure.Parameters.Length}];");
        method.Statements.Add(parametersExpr);
        for (var i = 0; i < _procedure.Parameters.Length; i++)
        {
            var param = _procedure.Parameters[i];
            var variableName= param.IsOut?@$"{param.Name}_Out":param.Name;
            method.Statements.Add(new CodeSnippetStatement(@$"var {variableName} = new SqlParameter(""@{param.Name}"", args.{param.Name});"));
            if (param.IsOut)
            {
                method.Statements.Add(new CodeSnippetStatement(@$"{variableName}.Direction = ParameterDirection.Output;"));
            }
            method.Statements.Add(new CodeSnippetStatement(@$"parameters[{i}]= {variableName};"));
            //Empty line
            method.Statements.Add(new CodeSnippetStatement(""));
        }
    }

    private void MultiResultHandling(CodeMemberMethod executeMethod)
    {
        var resultDeclaration = new CodeVariableDeclarationStatement(
            "var",
            "result",
            new CodeObjectCreateExpression(Utility.ReturnTypeClassName(_procedure)));
        executeMethod.Statements.Add(resultDeclaration);

        executeMethod.Statements.Add(new CodeSnippetStatement("using (var connection = db.GetSqlServerConnection())"));
        //Create the inner 'command' statement
        executeMethod.Statements.Add(new CodeSnippetStatement("{"));
        executeMethod.Statements.Add(new CodeSnippetStatement(@$"using (var command = new SqlCommand(""{_procedure.Name}"", connection))"));
        executeMethod.Statements.Add(new CodeSnippetStatement("{"));

        executeMethod.Statements.Add(new CodeSnippetStatement("command.CommandType = CommandType.StoredProcedure;"));
        executeMethod.Statements.Add(new CodeSnippetStatement("command.Parameters.AddRange(parameters);"));

        executeMethod.Statements.Add(new CodeSnippetStatement("using (var reader = command.ExecuteReader())"));
        executeMethod.Statements.Add(new CodeSnippetStatement("{"));
        for (var i = 0; i < _procedure.Dependencies.Length; i++)
        {
            var childProc = _procedure.Dependencies[i].ChildProcedure;
            ///TODO add logic here - Repeat
            executeMethod.Statements.Add(new CodeCommentStatement(@$"***{childProc}***"));
            executeMethod.Statements.Add(new CodeSnippetStatement("while (reader.Read())"));
            executeMethod.Statements.Add(new CodeSnippetStatement("{"));
            executeMethod.Statements.Add(new CodeSnippetStatement(@$"result.resultSet{i} = reader.Fill<{Utility.ReturnTypeClassName(childProc)}>().FirstOrDefault();"));

            // Read and fill each resultset
            executeMethod.Statements.Add(new CodeSnippetStatement("}"));
            executeMethod.Statements.Add(new CodeSnippetStatement("reader.NextResult();"));
            executeMethod.Statements.Add(new CodeSnippetStatement(""));
        }
        // End repeat
        executeMethod.Statements.Add(new CodeSnippetStatement("}"));

        executeMethod.Statements.Add(new CodeSnippetStatement("}"));
        executeMethod.Statements.Add(new CodeSnippetStatement("}"));
    }

    private void ExtractOutParametersValues(CodeMemberMethod method)
    {
        if (_procedure.Parameters?.Length > 0)
        {
            // Extract output values from params
            foreach (var param in _procedure.Parameters.Where(p => p.IsOut))
            {
                // Creating if (UpdatedDataCacheID_Out.Value != DBNull.Value)
                var paramValue = new CodeVariableReferenceExpression(@$"{param.Name}_Out.Value");
                var condition = new CodeBinaryOperatorExpression(
            paramValue,
            CodeBinaryOperatorType.IdentityInequality,
            new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DBNull"), "Value")
        );

                var ifStatement = new CodeConditionStatement(
            condition,
            new CodeStatement[]
            {
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("args"), param.Name),
                    new CodeCastExpression(Utility.MapType(param.Type), paramValue)
                )
            });
                method.Statements.Add(ifStatement);
            }
        }
    }
    private void ExecuteMethod(CodeTypeDeclaration parentClass, CodeTypeDeclaration parametersClass, CodeTypeDeclaration returnTypeClass)
    {
        // Add a simple method to the class
        var executeMethod = new CodeMemberMethod
        {
            Name = "Execute",
            ReturnType = returnTypeClass is null ? voidType : new CodeTypeReference(Utility.ReturnTypeClassName(_procedure)),
            Attributes = MemberAttributes.Static | MemberAttributes.Public

        };
        // Parameters
        executeMethod.Parameters.Add(Utility.GetParameter("IDatabase", "db"));
        if (parametersClass is not null)
        {
            executeMethod.Parameters.Add(Utility.GetParameter(Utility.ParametersClassName(_procedure), "args"));
        }
        if (_procedure.Parameters?.Length > 0)
        {
            AddParameters(executeMethod);
        }
        // Execute Method Body
        //Return Statement
        if (_procedure.ReturnType is not null)
        {
            if (_procedure.ReturnType[0].IsMultiResult == 1)
            {
                MultiResultHandling(executeMethod);
            }
            else
            {
                executeMethod.Statements.Add(new CodeSnippetStatement(@$"var result = Helpers.ExecuteStoredProcedureWithReaderAsync<{Utility.ReturnTypeClassName(_procedure)}>(db, ""{_procedure.Schema}.{_procedure.Name}"", parameters).GetAwaiter().GetResult().FirstOrDefault();"));
            }
            ExtractOutParametersValues(executeMethod);
            //return statement
            var returnStatement = new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result"));
            executeMethod.Statements.Add(returnStatement);
        }
        else
        {
            executeMethod.Statements.Add(new CodeSnippetStatement(@$"Helpers.ExecuteStoredProcedureAsync(db, ""{_procedure.Schema}.{_procedure.Name}"", parameters).GetAwaiter().GetResult();"));
            ExtractOutParametersValues(executeMethod);
        }
        parentClass.Members.Add(executeMethod);
    }
    private void BuildClass()
    {
        AddImportStatements();
        CreateNamespace();

        var parametersClass = InputArgumentsClass();
        var returnTypeClass = ReturnTypeClass();
        var parentClass = Utility.GetClass(_procedure.Name);
        _codeNamespace.Types.Add(parentClass);

        ExecuteMethod(parentClass, parametersClass, returnTypeClass);
    }
    public void Emit(string outputFolder)
    {
        BuildClass();
        var content = Utility.GetCSharpCode(_compileUnit);
        Utility.WriteFile(Path.Join(outputFolder, _outputFileName), content);
    }
}




