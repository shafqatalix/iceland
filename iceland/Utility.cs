using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.CSharp;

public class Utility
{
	public static string Version = Assembly.GetEntryAssembly()?
								   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
								   .InformationalVersion
								   .ToString()??"";
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
		""Procedures"":[##PROCS##],
		""UserDefinedTypes"":[##TYPES##] 
		}}
		";
		var data=new StringBuilder();
		foreach (var i in metadata.Procedures)
		{
			data.Append(i.ToJson());
			data.Append(",");
		}
		result = result.Replace("##PROCS##", data.ToString());

		data = new StringBuilder();
		foreach (var i in metadata.UserDefinedTypes)
		{
			data.Append(i.ToJson());
			data.Append(",");
		}
		result = result.Replace("##TYPES##", data.ToString());

		return result;
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

	public static Type? MapType(string input)
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
			case "nchar":
			case "xml":
			case "char":
			case "sysname":
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
			case "bigint":
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

		return null;
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


	public static Config GetConfig()
	{
		if (File.Exists("./local.json"))
		{
			return JsonSerializer.Deserialize<Config>(File.ReadAllText("./local.json"));
		}
		if (File.Exists("./config.json"))
		{
			return JsonSerializer.Deserialize<Config>(File.ReadAllText("./config.json"));
		}
		else
		{
			Console.WriteLine("No config file found");
		}
		return null;
	}
}
