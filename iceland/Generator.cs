
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
			var outputFolder = Utility.CreateDirectory($@"./generated/{dbName}");

			/*
			Generate Static code
			*/
			new Csproj().Emit(@$"{outputFolder}/{dbName}.csproj");
			new Contracts().Emit(outputFolder);
			new ExtensionMethods().Emit(outputFolder);
			new Helper().Emit(outputFolder);
			/*
			Generate Meta
			*/
			var meta=DbExecute.GetMeta();
			meta.Database = dbName;
			Utility.WriteFile(@$"{outputFolder}/meta.json", Utility.ToJson(meta));

			//UDTs
			Utility.CreateDirectory($@"{outputFolder}/UserDefinedTypes");
			foreach (var udt in meta.UserDefinedTypes)
			{
				new Udt(udt).Emit(@$"{outputFolder}/UserDefinedTypes");
			}

			//Procedures
			Utility.CreateDirectory($@"{outputFolder}/Procedures");

			foreach (var proc in meta.Procedures)
			{
				var outPath=proc.IsMultiResultSet() ? @$"{outputFolder}/Procedures/MultiResultSet" : @$"{outputFolder}/Procedures";

				new Proc(dbName, proc, meta).Emit(outPath);
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
