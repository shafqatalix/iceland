using System.Reflection;

internal class Program
{
    static void Main(string[] args)
    {
        Console.Clear();

        Console.WriteLine($"Iceland v{Utility.Version}");

        if (args.Length > 0)
        {
            var  connectionString = args[1];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("No argument provided or config file is provided");
                Console.WriteLine(Environment.CurrentDirectory);
                return;
            }
            Generator.Run(
           connectionString
       );
        }
        else
        {
            var config= Utility.GetConfig();

            foreach (var project in config.Projects)
            {
                var connectionString = project.ConnectionString;

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    Console.WriteLine("No argument provided or config file is provided");
                    Console.WriteLine(Environment.CurrentDirectory);
                    return;
                }
                Generator.Run(
                    connectionString
                );
            }
        }
    }
}