using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;
using Microsoft.CSharp;

public class CodeDomProjectExample
{
    public static void Main()
    {
        // Create the XML writer settings for indentation
        var settings = new XmlWriterSettings { Indent = true };

        using (var writer = XmlWriter.Create("GeneratedProject.csproj", settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("Project");
            writer.WriteAttributeString("Sdk", "Microsoft.NET.Sdk");

            // PropertyGroup element
            writer.WriteStartElement("PropertyGroup");
            writer.WriteElementString("TargetFramework", "net8.0");
            writer.WriteElementString("ImplicitUsings", "disable");
            writer.WriteElementString("Nullable", "enable");
            writer.WriteElementString("NoWarn", "CS1591,CS1587,CS1591");
            writer.WriteElementString("Version", "0.0.1-alpha");
            writer.WriteEndElement(); // PropertyGroup

            // First ItemGroup element with AdditionalFiles
            writer.WriteStartElement("ItemGroup");
            writer.WriteStartElement("AdditionalFiles");
            writer.WriteAttributeString("Include", "$([MSBuild]::GetPathOfFileAbove(BannedSymbols.txt))");
            writer.WriteEndElement(); // AdditionalFiles
            writer.WriteEndElement(); // ItemGroup

            // Second ItemGroup element with PackageReference
            writer.WriteStartElement("ItemGroup");
            writer.WriteStartElement("PackageReference");
            writer.WriteAttributeString("Include", "Microsoft.Data.SqlClient");
            writer.WriteAttributeString("Version", "5.2.2");
            writer.WriteEndElement(); // PackageReference
            writer.WriteEndElement(); // ItemGroup

            writer.WriteEndElement(); // Project
            writer.WriteEndDocument();
        }

        Console.WriteLine("Project file generated successfully.");
    }
}
