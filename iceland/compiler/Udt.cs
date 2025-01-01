using System.CodeDom;

public class Udt
{
    private readonly UTD _udt;
    private readonly string _outputFileName;

    public Udt(UTD udt)
    {
        _udt = udt;
        _outputFileName = @$"{_udt.DisplayName}.cs";
    }
    public string Build()
    {
        CodeCompileUnit compileUnit = new CodeCompileUnit();

        // Create a new namespace
        CodeNamespace codeNamespace = new CodeNamespace("UserDefinedTypes");
        codeNamespace.Imports.Add(new CodeNamespaceImport("System"));

        // Declare the class
        var classType = new CodeTypeDeclaration(_udt.DisplayName)
        {
            IsClass = true,
            TypeAttributes = System.Reflection.TypeAttributes.Public
        };

        // Add the class to the namespace
        codeNamespace.Types.Add(classType);

        if (_udt.Fields is not null)
        {
            foreach (var field in _udt.Fields)
            {
                // Declare the ID property
                var property = new CodeMemberField
                {
                    Name = field.Name,
                    Type = new CodeTypeReference(Utility.MapType(field.Type)),
                    Attributes = MemberAttributes.Public,
                    //HasGet = true,
                    //HasSet = true
                };

                // Add the property to the class
                classType.Members.Add(property);
            }
        }

        // Add the namespace to the compile unit
        compileUnit.Namespaces.Add(codeNamespace);

        // Generate and output the code         
        return Utility.GetCSharpCode(compileUnit);
    }

    public void Emit(string outputFolder)
    {
        var content=Build();
        Utility.WriteFile(Path.Join(outputFolder, _outputFileName), content);
    }
}
