
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

internal class Helper
{
    readonly  CodeDomProvider provider = new CSharpCodeProvider();
    readonly CodeSnippetCompileUnit _cscu;

    private string _code=@$"        
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Helpers
{{
    
    public static async Task ExecuteStoredProcedureAsync(IDatabase database, string storedProcedureName, params SqlParameter[] parameters)
    {{
        using (var connection = database.GetSqlServerConnection())
        {{
            //await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
            {{
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(parameters);

                // Execute the stored procedure asynchronously
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }}
        }}
    }}


    public static async Task<IEnumerable<T>> ExecuteStoredProcedureWithReaderAsync<T>(IDatabase database, string storedProcedureName, params SqlParameter[] parameters) where T : new()
    {{
        using (var connection = database.GetSqlServerConnection())
        {{
            //await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
            {{
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(parameters);

                // Execute the stored procedure and read the results asynchronously
                using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {{
                    var result= reader.Fill<T>();
                    return result;
                }}
            }}
       }}
    }}
}}

";

    public Helper()
    {
        _cscu = new CodeSnippetCompileUnit(_code);
    }

    private string Build()
    {
        return Utility.GetCSharpCode(_cscu);
    }

    public void Emit(string outputFolder)
    {
        var content=Build();
        Utility.WriteFile(Path.Join(@$"{outputFolder}", @$"Helper.cs"), content);
    }
}