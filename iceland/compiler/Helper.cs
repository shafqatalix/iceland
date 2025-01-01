
using System.CodeDom;


internal class Helper
{
    private readonly CodeSnippetCompileUnit _cscu;

    private readonly string _code=@$"        
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

internal class Helpers
{{
    internal static string Version = Assembly.GetEntryAssembly()?
								   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
								   .InformationalVersion
								   .ToString()?? string.Empty;

    internal static async Task<SqlConnection> GetConnectionAsync(IDatabase database)
    {{
        var connection = database.GetSqlServerConnection();

        if (connection.State == ConnectionState.Closed)
        {{
            await connection.OpenAsync();
        }}

        return connection;
    }}
    internal static SqlCommand GetCommand(SqlConnection connection, string storedProcedureName, params SqlParameter[] parameters)
    {{
        var command = new SqlCommand(storedProcedureName, connection);
        command.CommandType = CommandType.StoredProcedure;
        if (parameters is not null && parameters.Length > 0)
        {{
            command.Parameters.AddRange(parameters);
        }}
        return command;
    }}
    internal static void TraceConnection(Activity? span, SqlConnection? connection)
    {{
        span?.AddTag(""connection.Database"", connection?.Database);
        span?.AddTag(""connection.DataSource"", connection?.DataSource);
    }}
    internal static void TraceCommand(Activity? span, SqlCommand? command)
    {{
        span?.AddTag(""command.CommandType"", command?.CommandType);
        span?.AddTag(""command.CommandText"", command?.CommandText);
        span?.AddTag(""command.CommandTimeout"", command?.CommandTimeout);
    }}

   internal static void TraceError(Activity? span, ILogger? _logger, Exception exp, string storedProcedureName, params SqlParameter[] parameters)
    {{
        span?.AddTag(""Error.Message"", exp.Message);
        _logger?.LogError(exp.Message);

        span?.AddTag(@$""{{storedProcedureName}} -> Parameters"", string.Empty);
        _logger?.LogError(@$""{{storedProcedureName}} -> Parameters"", string.Empty);
        foreach (var p in parameters)
        {{
            span?.AddTag(p.ParameterName, p.Value);
            _logger?.LogError(@$""Parameter: {{p.ParameterName}} , Value: ${{p.Value}}"");
        }}
        span?.AddTag(""Error.StackTrace"", exp.StackTrace);
        _logger?.LogError(exp.StackTrace);
    }}
    internal static async Task ExecuteStoredProcedureAsync(IDatabase database, ActivitySource trace, ILogger _logger, string storedProcedureName, params SqlParameter[] parameters)
    {{
        var clock = Stopwatch.StartNew();
        _logger?.LogInformation(@$""Executing Procedure: {{storedProcedureName}}"");
        var span=trace.CreateActivity(@$""Executing Procedure: {{storedProcedureName}}"",ActivityKind.Client);

        try
        {{
            using (var connection = await GetConnectionAsync(database))
            {{
                TraceConnection(span, connection);
                using (var command = GetCommand(connection, storedProcedureName, parameters))
                {{
                    TraceCommand(span, command);
                    // Execute the stored procedure asynchronously
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    span?.AddTag(""TotalTime"", @$""{{clock.Elapsed.TotalMilliseconds}}ms"");
                    clock?.Stop();
                }}
            }}
        }}
        catch (Exception exp)
        {{
            TraceError(span, _logger, exp, storedProcedureName, parameters);
            clock?.Stop();
        }}
    }}
    internal static async Task<IEnumerable<T>> ExecuteStoredProcedureWithReaderAsync<T>(IDatabase database, ActivitySource trace, ILogger _logger, string storedProcedureName, params SqlParameter[] parameters) where T : new()
    {{
        var clock = Stopwatch.StartNew();
        _logger?.LogInformation(@$""Executing Procedure: {{storedProcedureName}}"");
        var span=trace.CreateActivity(@$""Executing Procedure: {{storedProcedureName}}"",ActivityKind.Client);

        try
        {{
            using (var connection = await GetConnectionAsync(database))
            {{
                TraceConnection(span, connection);
                using (var command = GetCommand(connection, storedProcedureName, parameters))
                {{
                    TraceCommand(span, command);
                    // Execute the stored procedure and read the results asynchronously
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                   {{
                        var result= await reader.Fill<T>().ConfigureAwait(false);
                        span?.AddTag(""TotalTime"", @$""{{clock.Elapsed.TotalMilliseconds}}ms"");
                        clock?.Stop();
                        return result;
                    }}
                }}
            }}
        }}
        catch (Exception exp)
        {{
            TraceError(span, _logger, exp, storedProcedureName, parameters);
            clock?.Stop();
            return null;
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