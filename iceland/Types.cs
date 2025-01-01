using System.Text.Json;

public class Field
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Type { get; set; }

}

public class Parameter
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Type { get; set; }
    public int Length { get; set; }
    public bool IsOut { get; set; }
    public bool IsUserDefinedType { get; set; }
    public bool IsUserDefinedTableType { get; set; }


}

public class ReturnType
{
    public ushort IsMultiResult { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsNullable { get; set; }
    public int Length { get; set; }
}
public class Dependency
{
    public string ChildProcedure { get; set; }
}
public class Procedure
{
    // public string ObjectId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Schema { get; set; }
    public Parameter[]? Parameters { get; set; }
    public ReturnType[]? ReturnType { get; set; }
    public Dependency[]? Dependencies { get; set; }

    public string ToJson()
    {
        var json= JsonSerializer.Serialize(this);
        return json;
    }

    public bool IsMultiResultSet()
    {
        return this.ReturnType is not null && this.ReturnType.Where(r => r?.IsMultiResult == 1).Count() > 0;
    }
}

public class UTD
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Schema { get; set; }
    public string DisplayName { get; set; }
    public Field[] Fields { get; set; }
    public bool IsTableType { get; set; }

    public string ToJson()
    {
        var json= JsonSerializer.Serialize(this);
        return json;
    }
}

public class MetaData
{

    public string Database { get; set; }

    public Procedure[] Procedures { get; set; }

    public UTD[] UserDefinedTypes { get; set; }

}


public class Project
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
}

public class Config
{
    public Project[] Projects { get; set; }
}