using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CSharp;

public static class Utility
{

	private static string GetAbsolutePath(string basePath)
	{
		return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, basePath));
	}

	public static void ReplaceInAllFiles(string directory, string find, string replace)
	{
		string rootfolder = GetAbsolutePath(directory);
		string[] files = Directory.GetFiles(rootfolder, "*.*", SearchOption.AllDirectories);
		foreach (string file in files)
		{
			string contents = File.ReadAllText(file);
			contents = contents.Replace(find, replace);        // Make files writable
			File.SetAttributes(file, FileAttributes.Normal);

			File.WriteAllText(file, contents);

		}
	}

	public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, string replaceTextInPath = "", string replaceWith = "")
	{
		var sourceDirAbsolutePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, sourceDir));
		var destinationDirAbsolutePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, destinationDir));

		// Get information about the source directory
		var dir = new DirectoryInfo(sourceDirAbsolutePath);

		// Check if the source directory exists
		if (!dir.Exists)
			throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

		// Cache directories before we start copying
		DirectoryInfo[] dirs = dir.GetDirectories();

		// Create the destination directory
		Directory.CreateDirectory(destinationDirAbsolutePath);

		// Get the files in the source directory and copy to the destination directory
		foreach (FileInfo file in dir.GetFiles())
		{
			string targetFilePath = Path.Combine(destinationDirAbsolutePath, file.Name).Replace(replaceTextInPath,replaceWith);
			file.CopyTo(targetFilePath, true);
		}

		// If recursive and copying subdirectories, recursively call this method
		if (recursive)
		{
			foreach (DirectoryInfo subDir in dirs)
			{
				string newDestinationDir = Path.Combine(destinationDir, subDir.Name).Replace(replaceTextInPath, replaceWith);
				CopyDirectory(subDir.FullName, newDestinationDir, true, replaceTextInPath, replaceWith);
			}

		}

	}

	public static void WriteFile(string filePath, string fileContents)
	{
		// Get information about the source directory
		var dir = new DirectoryInfo(Path.GetDirectoryName(filePath));

		// Check if the source directory exists
		if (!dir.Exists)
			Directory.CreateDirectory(dir.FullName);

		var outputPath = Path.Combine(dir.FullName, Path.GetFileName(filePath));
		File.WriteAllText(outputPath, fileContents);

	}

	public static string CreateDirectory(string path)
	{
		var absolutePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));
		// Get information about the source directory
		// var dir = new DirectoryInfo(Path.GetDirectoryName(absolutePath));

		// Check if the source directory exists
		if (!Directory.Exists(absolutePath))
		{
			Directory.CreateDirectory(absolutePath);
		}
		return absolutePath;
	}

	public static string ToJson(MetaData metadata)
	{
		var result=@$"
		{{
		""Database"":""{metadata.Database}"",
		""Procedures"":[##PROCS##] 
		}}
		";
		var procs=new StringBuilder();
		foreach (var i in metadata.Procedures)
		{
			procs.Append(i.ToJson());
			procs.Append(",");
		}
		return result.Replace("##PROCS##", procs.ToString());
	}

	public static string ReturnTypeClassName(Procedure procedure)
	{
		return @$"{procedure.Name}ReturnType";
	}

	public static string ReturnTypeClassName(string procedure)
	{
		return @$"{procedure}ReturnType";
	}

	public static string ParametersClassName(Procedure procedure)
	{
		return @$"{procedure.Name}Parameters";
	}

	/*
		public static Dictionary<string, ParameterTypeMap> typeMapper = new Dictionary<string, ParameterTypeMap> {
			{"bit", new ParameterTypeMap { ClrType = "bool?", DbType = "SqlDbType.Bit", LengthDivisor = null }},
			{"tinyint", new ParameterTypeMap { ClrType = "byte?", DbType = "SqlDbType.TinyInt", LengthDivisor = null }},
			{"smallint", new ParameterTypeMap { ClrType = "short?", DbType = "SqlDbType.SmallInt", LengthDivisor = null }},
			{"int", new ParameterTypeMap { ClrType = "int?", DbType = "SqlDbType.Int", LengthDivisor = null }},
			{"bigint", new ParameterTypeMap { ClrType = "long?", DbType = "SqlDbType.BigInt", LengthDivisor = null }},
			{"varchar", new ParameterTypeMap { ClrType = "string?", DbType = "SqlDbType.VarChar", LengthDivisor = 1 }},
			{"nvarchar", new ParameterTypeMap { ClrType = "string?", DbType = "SqlDbType.NVarChar", LengthDivisor = 2 }},
			{"xml", new ParameterTypeMap { ClrType = "string?", DbType = "SqlDbType.NVarChar", LengthDivisor = 2 }},
			{"char", new ParameterTypeMap { ClrType ="string?" , DbType = "SqlDbType.Char", LengthDivisor = 1 }},
			{"nchar", new ParameterTypeMap { ClrType = "string?", DbType = "SqlDbType.NChar", LengthDivisor = 2 }},
			{"date", new ParameterTypeMap { ClrType = "DateTime?", DbType = "SqlDbType.Date", LengthDivisor = null }},
			{"datetime", new ParameterTypeMap { ClrType = "DateTime?", DbType = "SqlDbType.DateTime", LengthDivisor = null }},
			{"datetime2", new ParameterTypeMap { ClrType = "DateTime?", DbType = "SqlDbType.DateTime2", LengthDivisor = null }},
			{"smalldatetime", new ParameterTypeMap { ClrType = "DateTime?", DbType = "SqlDbType.SmallDateTime", LengthDivisor = null }},
			{"datetimeoffset", new ParameterTypeMap { ClrType = "DateTimeOffset?", DbType = "SqlDbType.DateTimeOffset", LengthDivisor = null }},
			{"timestamp", new ParameterTypeMap { ClrType = "DateTime?", DbType = "SqlDbType.SmallDateTime", LengthDivisor = null }},
			{"time", new ParameterTypeMap { ClrType = "TimeSpan?", DbType = "SqlDbType.Time", LengthDivisor = null }},
			{"varbinary", new ParameterTypeMap { ClrType = "byte[]?", DbType = "SqlDbType.VarBinary", LengthDivisor = null }},
			{"binary", new ParameterTypeMap { ClrType = "byte[]?", DbType = "SqlDbType.VarBinary", LengthDivisor = null }},
			{"money", new ParameterTypeMap { ClrType = "decimal?", DbType = "SqlDbType.Money", LengthDivisor = null }},
			{"numeric", new ParameterTypeMap { ClrType = "decimal?", DbType = "SqlDbType.Decimal", LengthDivisor = null }},
			{"decimal", new ParameterTypeMap { ClrType = "decimal?", DbType = "SqlDbType.Decimal", LengthDivisor = null }},
			{"real", new ParameterTypeMap { ClrType = "float?", DbType = "SqlDbType.Real", LengthDivisor = null }},
			{"float", new ParameterTypeMap { ClrType = "double?", DbType = "SqlDbType.Float", LengthDivisor = null }},
			{"uniqueidentifier", new ParameterTypeMap { ClrType = "Guid?", DbType = "SqlDbType.UniqueIdentifier", LengthDivisor = null }}
		};
	*/
	public static Type MapType(string input)
	{

		// Clean ()
		var idx=input.IndexOf("(");
		if (idx >= 0)
		{
			input = input.Remove(idx);
		}

		switch (input)
		{
			case "varchar":
			case "nvarchar":
			case "xml":
			case "char":
				return typeof(string);
			case "smallint":
				return typeof(short);
			case "int":
				return typeof(int);
			case "money":
			case "decimal":
			case "numeric":
				return typeof(decimal);
			case "float":
			case "real":
				return typeof(float);
			case "biting":
				return typeof(long);
			case "bit":
				return typeof(bool);
			case "tinyint":
				return typeof(byte);
			case "date":
			case "datetime":
			case "datetime2":
			case "smalldatetime":
				return typeof(DateTime);
			case "datetimeoffset":
				return typeof(DateTimeOffset);
			case "time":
				return typeof(TimeSpan);
			case "uniqueidentifier":
				return typeof(Guid);
			case "varbinary":
			case "binary":
				return typeof(byte[]);

		}

		return typeof(void);
	}

	public static CodeTypeDeclaration GetClass(string name) => new CodeTypeDeclaration(name)
	{
		IsClass = true
	};

	public static CodeParameterDeclarationExpression GetParameter(string type, string name) => new CodeParameterDeclarationExpression
	{
		Type = new CodeTypeReference(type),
		Name = name
	};

	public static string GetCSharpCode(CodeCompileUnit compileUnit)
	{
		using (var writer = new StringWriter())
		{
			var codeGenereationOptions = new CodeGeneratorOptions()
			{
				BlankLinesBetweenMembers = false,
				IndentString = "  ",
				BracingStyle = "C"
			};
			var provider = new CSharpCodeProvider();
			provider.GenerateCodeFromCompileUnit(compileUnit, writer, codeGenereationOptions);
			return writer.GetStringBuilder().ToString();
		}
	}
}
