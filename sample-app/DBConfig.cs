

using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

public class DbConfig : IDatabase
{
    public SqlConnection GetSqlServerConnection()
    {
        var ConnectionString="Server=127.0.0.1;Initial Catalog=SampleDB; Encrypt=False; Persist Security Info=False; TrustServerCertificate=True; User=sa; pwd=yourStrongPassword@12345";

        return new SqlConnection(ConnectionString);
    }
}

public class Logger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Console.WriteLine(new { logLevel, eventId, state, exception });
}