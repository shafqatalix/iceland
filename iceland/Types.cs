using System.Text.Json;

public class Field
{
    public string Name { get; set; }
    public string DbType { get; set; }
    public string ClrType { get; set; }

}

public class Parameter
{
    public string Name { get; set; }
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
    public string Schema { get; set; }
    public Parameter[]? Parameters { get; set; }
    public ReturnType[]? ReturnType { get; set; }
    public Dependency[]? Dependencies { get; set; }

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

}
