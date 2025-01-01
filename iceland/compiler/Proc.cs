
using System.CodeDom;

public class Proc
{
    private readonly CodeCompileUnit _compileUnit = new CodeCompileUnit();
    private CodeNamespace _codeNamespace;
    private readonly Procedure _procedure;
    private readonly string _outputFileName;
    private readonly string _databaseName;
    private readonly MetaData _meta;

    public Proc(string databaseName, Procedure procedure, MetaData meta)
    {
        _procedure = procedure;
        _databaseName = databaseName;
        _outputFileName = @$"{procedure.DisplayName}.cs";
        _meta = meta;

    }

    private CodeMemberField GetCodeMemberField(string name, string type)
    {
        var mappedType = Utility.MapType(type);
        if (mappedType is null)
        {
            var udt=_meta.UserDefinedTypes.Where(u=> u.Name== type).FirstOrDefault();
            if (udt is null)
            {
                throw new Exception(@$"Could not find type for name:${name}, type:${type}");
            }
            var  field = new CodeMemberField(udt.DisplayName, name);
            field.Attributes = MemberAttributes.Public;
            return field;
        }
        else
        {
            var field = new CodeMemberField(mappedType, name);
            field.Attributes = MemberAttributes.Public;
            return field;
        }
    }
    private CodeSnippetTypeMember GetCodeMemberField2(string name, string type)
    {
        var mappedType = Utility.MapType(type);
        if (mappedType is null)
        {
            var udt=_meta.UserDefinedTypes.Where(u=> u.Name== type).FirstOrDefault();
            if (udt is null)
            {
                throw new Exception(@$"Could not find type for name:${name}, type:${type}");
            }
            var  field = new CodeSnippetTypeMember(@$"public {udt.DisplayName} {name} {{get;set;}}");
            //field.Attributes = MemberAttributes.Public;
            return field;
        }
        else
        {
            var  field = new CodeSnippetTypeMember(@$"public {mappedType} {name} {{get;set;}}");
            //field.Attributes = MemberAttributes.Public;
            return field;
        }
    }


    private void AddImportStatements()
    {
        CodeNamespace globalNamespace = new CodeNamespace();
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "UserDefinedTypes" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System.Linq" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System.Data" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System.Collections.Generic" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "Microsoft.Data.SqlClient" });
        globalNamespace.Imports.Add(new CodeNamespaceImport { Namespace = "System.Threading.Tasks" });
        globalNamespace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
        globalNamespace.Imports.Add(new CodeNamespaceImport("Microsoft.Extensions.Logging"));
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
                if (m.Type is null)
                    throw new Exception(@$"{_procedure.Name} -> Type is not defined for {m.Name}");

                currentClass.Members.Add(GetCodeMemberField2(m.Name, m.Type));

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
            var depsWithReturnTypes=GetDependenciesWithReturnTypes();

            for (var i = 0; i < depsWithReturnTypes.Length; i++)
            {
                var dep = Utility.ReturnTypeClassName(depsWithReturnTypes[i].ChildProcedure);
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
            currentClass.Members.Add(GetCodeMemberField2(m.Name, m.Type));

        }
        _codeNamespace.Types.Add(currentClass);
        return currentClass;
    }

    private void AddParameters(CodeMemberMethod method)
    {
        var parametersExpr=new CodeSnippetStatement(@$"var parameters=new SqlParameter[{_procedure.Parameters?.Length??0}];");
        method.Statements.Add(parametersExpr);
        for (var i = 0; i < _procedure.Parameters?.Length; i++)
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

    private Dependency[] GetDependenciesWithReturnTypes()
    {
        var depsWithReturnTypes= new List<Dependency>();
        if (_procedure.Dependencies is not null)
        {
            foreach (var dep in _procedure.Dependencies)
            {
                var proc=_meta.Procedures.Where(p=> p.Name.Equals(dep.ChildProcedure,StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (proc is not null && proc.ReturnType is not null && proc.ReturnType.Where(x => x.IsMultiResult == 0).Count() > 0)
                {
                    depsWithReturnTypes.Add(dep);
                }
            }
        }
        return depsWithReturnTypes.ToArray();
    }
    private void MultiResultHandling(CodeMemberMethod executeMethod)
    {
        var resultDeclaration = new CodeVariableDeclarationStatement(
            "var",
            "result",
            new CodeObjectCreateExpression(Utility.ReturnTypeClassName(_procedure)));
        executeMethod.Statements.Add(resultDeclaration);

        executeMethod.Statements.Add(new CodeSnippetStatement("using (var connection = _database.GetSqlServerConnection())"));
        //Create the inner 'command' statement
        executeMethod.Statements.Add(new CodeSnippetStatement("{"));
        executeMethod.Statements.Add(new CodeSnippetStatement("Helpers.TraceConnection(span, connection);"));
        executeMethod.Statements.Add(new CodeSnippetStatement(@$"using (var command = new SqlCommand(""[{_procedure.Schema}].[{_procedure.Name}]"", connection))"));
        executeMethod.Statements.Add(new CodeSnippetStatement("{"));
        executeMethod.Statements.Add(new CodeSnippetStatement("Helpers.TraceCommand(span, command);"));
        executeMethod.Statements.Add(new CodeSnippetStatement("command.CommandType = CommandType.StoredProcedure;"));
        executeMethod.Statements.Add(new CodeSnippetStatement("command.Parameters.AddRange(parameters);"));

        executeMethod.Statements.Add(new CodeSnippetStatement("using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))"));
        executeMethod.Statements.Add(new CodeSnippetStatement("{"));


        var depsWithReturnTypes=GetDependenciesWithReturnTypes();
        for (var i = 0; i < depsWithReturnTypes.Length; i++)
        {
            var childProc = depsWithReturnTypes[i].ChildProcedure;
            executeMethod.Statements.Add(new CodeSnippetStatement(@$"span?.AddTag(""ResultSet#{i}"", ""Result of -> {childProc}"");"));
            ///TODO add logic here - Repeat
            executeMethod.Statements.Add(new CodeCommentStatement(@$"***{childProc}***"));
            executeMethod.Statements.Add(new CodeSnippetStatement("while (await reader.ReadAsync().ConfigureAwait(false))"));
            executeMethod.Statements.Add(new CodeSnippetStatement("{"));
            executeMethod.Statements.Add(new CodeSnippetStatement(@$"var resultSet{i} = await reader.Fill<{Utility.ReturnTypeClassName(childProc)}>().ConfigureAwait(false);"));
            executeMethod.Statements.Add(new CodeSnippetStatement(@$"result.resultSet{i} = resultSet{i}.FirstOrDefault();"));

            // Read and fill each resultset
            executeMethod.Statements.Add(new CodeSnippetStatement("}"));
            executeMethod.Statements.Add(new CodeSnippetStatement("await reader.NextResultAsync().ConfigureAwait(false);"));
            executeMethod.Statements.Add(new CodeSnippetStatement(""));
        }
        // End repeat
        executeMethod.Statements.Add(new CodeSnippetStatement("}"));

        executeMethod.Statements.Add(new CodeSnippetStatement(@$"span?.AddTag(""TotalTime"", @$""{{ clock.Elapsed.TotalMilliseconds }}ms"");"));
        executeMethod.Statements.Add(new CodeSnippetStatement("clock?.Stop();"));

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
            Name = "ExecuteAsync",
            ReturnType = new CodeTypeReference( returnTypeClass is null? "async Task": @$"async Task<{Utility.ReturnTypeClassName(_procedure)}>"),
            //ReturnType = returnTypeClass is null ? voidType : new CodeTypeReference(Utility.ReturnTypeClassName(_procedure)),
            Attributes =  MemberAttributes.Public

        };

        // Parameters
        //executeMethod.Parameters.Add(Utility.GetParameter("IDatabase", "db"));
        if (parametersClass is not null)
        {
            executeMethod.Parameters.Add(Utility.GetParameter(Utility.ParametersClassName(_procedure), "args"));
        }

        AddParameters(executeMethod);

        // Execute Method Body
        //Return Statement
        if (_procedure.ReturnType is not null)
        {
            if (_procedure.ReturnType[0].IsMultiResult == 1)
            {
                executeMethod.Statements.Add(new CodeSnippetStatement("var clock = Stopwatch.StartNew();"));
                executeMethod.Statements.Add(new CodeSnippetStatement(@$"var span=_activity.CreateActivity(""ExecuteNonQueryAsync for Procedure: {_procedure.Name}"",ActivityKind.Client);"));

                //Try block
                executeMethod.Statements.Add(new CodeSnippetStatement("try{"));

                MultiResultHandling(executeMethod);
                ExtractOutParametersValues(executeMethod);
                //return statement 
                executeMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")));
                //End try
                executeMethod.Statements.Add(new CodeSnippetStatement("}"));
                // Catch block
                executeMethod.Statements.Add(new CodeSnippetStatement("catch(Exception exp){"));
                executeMethod.Statements.Add(new CodeSnippetStatement(@$"Helpers.TraceError(span, _logger, exp, ""{_procedure.Name}"", parameters);"));
                executeMethod.Statements.Add(new CodeSnippetStatement("clock?.Stop();"));
                executeMethod.Statements.Add(new CodeSnippetStatement("return null;"));
                executeMethod.Statements.Add(new CodeSnippetStatement("}"));
            }
            else
            {
                executeMethod.Statements.Add(new CodeSnippetStatement(@$"var result = await Helpers.ExecuteStoredProcedureWithReaderAsync<{Utility.ReturnTypeClassName(_procedure)}>(_database, _activity, _logger, ""[{_procedure.Schema}].[{_procedure.Name}]"", parameters);"));
                ExtractOutParametersValues(executeMethod);
                //return statement 
                executeMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result.FirstOrDefault()")));
            }
        }
        else
        {
            executeMethod.Statements.Add(new CodeSnippetStatement(@$"await Helpers.ExecuteStoredProcedureAsync(_database, _activity,_logger, ""[{_procedure.Schema}].[{_procedure.Name}]"", parameters);"));
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
        var parentClass = Utility.GetClass(_procedure.DisplayName);
        _codeNamespace.Types.Add(parentClass);

        // Declare the private readonly fields
        CodeMemberField databaseField = new CodeMemberField("IDatabase", "_database")
        {
            Attributes = MemberAttributes.Private | MemberAttributes.Final
        };
        parentClass.Members.Add(databaseField);

        CodeMemberField loggerField = new CodeMemberField("ILogger", "_logger")
        {
            Attributes = MemberAttributes.Private | MemberAttributes.Final
        };
        parentClass.Members.Add(loggerField);

        CodeMemberField activityField = new CodeMemberField("ActivitySource", "_activity")
        {
            Attributes = MemberAttributes.Private | MemberAttributes.Final
        };
        parentClass.Members.Add(activityField);

        // Declare the constructor
        CodeConstructor constructor = new CodeConstructor
        {
            Attributes = MemberAttributes.Public
        };
        constructor.Parameters.Add(new CodeParameterDeclarationExpression("IDatabase", "database"));
        constructor.Parameters.Add(new CodeParameterDeclarationExpression("ILogger", "logger = null"));

        constructor.Statements.Add(new CodeAssignStatement(
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_database"),
            new CodeArgumentReferenceExpression("database")));

        constructor.Statements.Add(new CodeAssignStatement(
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_logger"),
            new CodeArgumentReferenceExpression("logger")));

        constructor.Statements.Add(new CodeAssignStatement(
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_activity"),
            new CodeObjectCreateExpression("ActivitySource",
                new CodePrimitiveExpression("StoredProcedure"),
                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Helpers"), "Version")
            // new CodeObjectCreateExpression("Dictionary<string, object>",
            //     new CodeExpression[]
            //     {
            //         new CodePrimitiveExpression( _procedure.Name),
            //         new CodePrimitiveExpression(_procedure.Name)
            //     }
            // )
            )
        ));

        parentClass.Members.Add(constructor);

        //Methods
        ExecuteMethod(parentClass, parametersClass, returnTypeClass);
    }
    public void Emit(string outputFolder)
    {
        BuildClass();
        var content = Utility.GetCSharpCode(_compileUnit);
        Utility.WriteFile(Path.Join(outputFolder, _outputFileName), content);
    }
}




