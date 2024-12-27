using System.Reflection;


internal class Program
{
    static void Main(string[] args)
    {
        Console.Clear();
        // var x=Utility.MapType("decimal(14,4)");
        // Console.WriteLine(x);
        // return;
        var versionString = Assembly.GetEntryAssembly()?
                                   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                   .InformationalVersion
                                   .ToString();

        Console.WriteLine($"Iceland v{versionString}");

        if (args.Length == 0)
        {
            Console.WriteLine("No argument provided");
            return;
        }

        var connectionString = args[1];

        Console.WriteLine(@$"ConnectionString: {connectionString}");

        Generator.Run(
            connectionString
        );

    }
}