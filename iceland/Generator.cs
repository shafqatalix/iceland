
public class Generator
{
	public static void Run(string connectionString)
	{
		Console.WriteLine("Executing  Run()");
		try
		{
			Console.WriteLine($@"Generating code for ConnectionString -> {connectionString}");
			DbExecute.ConnectionString = connectionString;
			var dbName = DbExecute.GetDBName();
			var outputProjectName =dbName;
			var outputFolder = Utility.CreateDirectory($@"./{outputProjectName}");

			/*
			Generate Static code
			*/
			new Csproj().Emit(@$"{outputFolder}/{outputProjectName}.csproj");
			new Contracts().Emit(outputFolder);
			new ExtensionMethods().Emit(outputFolder);
			new Helper().Emit(outputFolder);
			/*
			Generate Meta
			*/
			var meta=DbExecute.GetMeta();
			meta.Database = dbName;
			Utility.CreateDirectory($@"{outputFolder}/Procedures");
			Utility.WriteFile(@$"{outputFolder}/meta.json", Utility.ToJson(meta));
			foreach (var proc in meta.Procedures)
			{
				new Proc(dbName, proc).Emit(@$"{outputFolder}/Procedures");
			}

			Console.WriteLine($@"Code Generation successful -> {connectionString}\n");
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Console.WriteLine($@"Code Generation failed -> {connectionString}\n");
		}

	}
}
