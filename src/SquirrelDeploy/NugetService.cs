using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SquirrelDeploy
{
    public sealed class NugetService
    {
        private static readonly string tempDirName = "temp";
        private static readonly string nuspecFileName = "package.nuspec";
        
        private readonly MiscFunctions _miscFunctions;
        
        public NugetService(MiscFunctions miscFunctions)
        {
            if (miscFunctions == null)
                throw new ArgumentNullException(nameof(miscFunctions));

            _miscFunctions = miscFunctions;
        }

        public string GeneratePackage(string sourceDir, string projectName, string buildConfiguration)
        {
            if (!Directory.Exists(sourceDir))
                throw new ArgumentException($"Source directory not found: {sourceDir}");

            string projectFilePath = FindProjectFilePath(sourceDir, projectName);

            if (projectFilePath == null)
                throw new ArgumentException($"Project file not found for deployment project: {projectName}");

            string appPath = FindBuildOutputPath(projectFilePath, buildConfiguration);

            if (appPath == null)
                throw new Exception($"Build output path not found for project file: {projectFilePath}");

            if (!Directory.Exists(appPath))
                throw new Exception($"Build output path found but directory does not exist: {appPath}");

            InitTempDirectory();

            var nuspecFilePath = Path.Combine(tempDirName, nuspecFileName);

            WriteNuspecFile(nuspecFilePath, appPath);
            var nupkgFilePath = PackNuspecFile(nuspecFilePath);

            if (nupkgFilePath == null)
                throw new Exception($"Can't find nupkg-file after running pack.");
            
            return nupkgFilePath;
        }

        public void CleanUp()
        {
            DeleteTempDir();
        }

        private string FindProjectFilePath(string sourceDir, string projectToDeploy)
        {
            var projectFilePath =
                Directory.EnumerateFiles(sourceDir, projectToDeploy, SearchOption.AllDirectories).FirstOrDefault();

            return projectFilePath;
        }

        private string FindBuildOutputPath(string projectFilePath, string buildConfiguration)
        {
            // Look in the project/msbuild file for the output directories of the build binaries.
            XDocument doc = XDocument.Load(projectFilePath);

            XNamespace ns = doc.Root.GetDefaultNamespace();

            var buildPaths =
                from e in doc.Descendants(ns + "PropertyGroup")
                let path = e.Element(ns + "OutputPath")?.Value
                let condition = e.Attribute("Condition")?.Value
                where path != null && condition != null
                let conditionArr = condition.Split(new[] { "==" }, StringSplitOptions.None)
                where conditionArr.Length == 2
                let configurationArr = conditionArr[1].Trim(' ', '\'').Split('|')
                where configurationArr.Length == 2
                select new
                {
                    Configuration = configurationArr[0],
                    OutputPath = path
                };

            // Find the output directory that matches the currently deployed build configuration (debug/release etc.).
            string outputPath = buildPaths.FirstOrDefault(p =>
                string.Equals(p.Configuration, buildConfiguration, StringComparison.OrdinalIgnoreCase))?.OutputPath;

            if (outputPath != null)
            {
                // If the path is absolute use that, otherwise combine project file path and found path to get the absolute path.
                return Path.IsPathRooted(outputPath)
                    ? outputPath
                    : Path.Combine(Path.GetDirectoryName(projectFilePath), outputPath);
            }

            return null;
        }

        private void WriteNuspecFile(string savePath, string appPath)
        {
            var doc = new XDocument(
                new XElement("package",
                    new XElement("metadata",
                        new XElement("id", "TestApp"),
                        new XElement("version", "1.2.3.4"),
                        new XElement("authors", "Peter"),
                        new XElement("description", "A small test app"),
                        new XElement("releaseNotes", "None")
                    ),
                    new XElement("files",
                        new XElement("file", new XAttribute("src", appPath), new XAttribute("target", @"lib\net45\"))
                    )
                )
            );

            using (var writer = new XmlTextWriter(savePath, null))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }
        }

        private string PackNuspecFile(string nuspecFilePath)
        {
            // Create nupkg-file in same folder as the nuspec-file.
            string outputDirectory = Path.GetDirectoryName(nuspecFilePath);

            var args = $"pack {nuspecFilePath} -OutputDirectory {outputDirectory}";
            _miscFunctions.RunProcess("nuget", args);

            // Find filename of the generated nupkg-file.
            string nupkgFilePath = Directory.EnumerateFiles(outputDirectory, "*.nupkg").SingleOrDefault();

            return nupkgFilePath != null ? 
                Path.GetFullPath(nupkgFilePath) : 
                null;
        }

        private void InitTempDirectory()
        {
            DeleteTempDir();
            Directory.CreateDirectory(tempDirName);
        }

        private void DeleteTempDir()
        {
            if (Directory.Exists(tempDirName))
            {
                Directory.Delete(tempDirName, true);
            }
        }
    }
}
