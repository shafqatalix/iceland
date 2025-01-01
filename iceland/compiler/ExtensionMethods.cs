
using System.CodeDom;

internal class ExtensionMethods
{
    private readonly CodeSnippetCompileUnit _compileUnit;


    private readonly string _code=@$"        
using System;
using Microsoft.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

internal static class Extensions
{{
    public static async Task<List<T>> Fill<T>(this SqlDataReader reader) where T : new()
    {{
        List<T> res = new List<T>();
        while (await reader.ReadAsync().ConfigureAwait(false))
        {{
            T t = new T();
            for (int inc = 0; inc < reader.FieldCount; inc++)
            {{
                var type = t.GetType();
                string name = reader.GetName(inc);
                var prop = type?.GetProperty(name);
                if (prop is not null)
                {{
                    if (name == prop.Name)
                    {{
                        var value = reader.GetValue(inc);
                        if (value != DBNull.Value)
                        {{
                            prop.SetValue(t, Convert.ChangeType(value, prop.PropertyType), null);
                        }}
                        //prop.SetValue(t, value, null);
                    }}
                }}
            }}
            res.Add(t);
        }}
        //reader.Close();
        return res;
    }}
}}
";
    public ExtensionMethods()
    {
        _compileUnit = new CodeSnippetCompileUnit(_code);
    }

    private string Build()
    {
        return Utility.GetCSharpCode(_compileUnit);
    }

    public void Emit(string outputFolder)
    {
        var content= Build();
        Utility.WriteFile(Path.Join(@$"{outputFolder}", @$"ExtensionMethods.cs"), content);
    }
}