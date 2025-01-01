
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;
using Microsoft.CSharp;

internal class Csproj
{
    public void Emit(string outputFile)
    {
        // Create the XML writer settings for indentation
        var settings = new XmlWriterSettings { Indent = true };

        using (var writer = XmlWriter.Create(outputFile, settings))
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

            // Second ItemGroup element with PackageReference
            writer.WriteStartElement("ItemGroup");
            writer.WriteStartElement("PackageReference");

            writer.WriteAttributeString("Include", "Microsoft.Data.SqlClient");
            writer.WriteAttributeString("Version", "5.2.2");
            writer.WriteEndElement(); // PackageReference

            writer.WriteStartElement("PackageReference");
            writer.WriteAttributeString("Include", "Microsoft.Extensions.Logging");
            writer.WriteAttributeString("Version", "9.0.0");
            writer.WriteEndElement(); // PackageReference

            writer.WriteEndElement(); // ItemGroup

            writer.WriteEndElement(); // Project
            writer.WriteEndDocument();
        }
    }
}
