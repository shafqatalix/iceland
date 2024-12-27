
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

internal class Contracts
{
    private readonly CodeNamespace _codeNamespace = new CodeNamespace("");

    private void IDatabase()
    {
        // Declare the interface
        var interfaceType = new CodeTypeDeclaration("IDatabase")
        {
            IsInterface = true
        };
        _codeNamespace.Types.Add(interfaceType);

        // Add a method to the interface
        var getSqlServerConnectionMethod = new CodeMemberMethod
        {
            Name = "GetSqlServerConnection",
            ReturnType = new CodeTypeReference("SqlConnection"),
            Attributes = MemberAttributes.Public
        };
        interfaceType.Members.Add(getSqlServerConnectionMethod);
    }
    private string Build()
    {
        var compileUnit = new CodeCompileUnit();
        // Create a new CodeNamespace and import necessary namespaces

        compileUnit.Namespaces.Add(_codeNamespace);
        _codeNamespace.Imports.Add(new CodeNamespaceImport("Microsoft.Data.SqlClient"));

        // Add IDatabase
        IDatabase();
        return Utility.GetCSharpCode(compileUnit);
    }

    public void Emit(string outputFolder)
    {
        var content=Build();
        Utility.WriteFile(Path.Join(@$"{outputFolder}", @$"Contracts.cs"), content);
    }
}
