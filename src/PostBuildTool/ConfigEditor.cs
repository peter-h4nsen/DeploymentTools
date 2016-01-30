using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PostBuildTool
{
    public sealed class ConfigEditor
    {
        private readonly string _appDir;
        private readonly string _configSettingsDir;
        private readonly Lazy<string> _configFilePath;

        public ConfigEditor(string appDir, string configSettingsDir)
        {
            if (string.IsNullOrWhiteSpace(appDir))
                throw new ArgumentNullException(nameof(appDir));

            if (string.IsNullOrWhiteSpace(configSettingsDir))
                throw new ArgumentNullException(nameof(configSettingsDir));

            _appDir = appDir;
            _configSettingsDir = configSettingsDir;
            _configFilePath = new Lazy<string>(FindConfigFilePath);
        }

        public void SetConnectionStrings()
        {
            string configFilePath = _configFilePath.Value;
            
            var doc = XDocument.Load(configFilePath);

            var connectionStringsElement = doc.Element("configuration")?.Element("connectionStrings");

            if (connectionStringsElement == null)
                throw new Exception("Connectionstrings element not found in config file.");

            connectionStringsElement.Elements().Remove();

            connectionStringsElement.Add(
                EnumerateConnectionStrings().Select(p =>
                    new XElement("add", new XAttribute("name", p.Item1), new XAttribute("connectionString", p.Item2))));

            doc.Save(configFilePath);
        }

        private string FindConfigFilePath()
        {
            const int takeCount = 2;
            var fileNames = Directory.EnumerateFiles(_appDir, "*.config").Take(takeCount).ToArray();

            if (fileNames.Length == 0)
                throw new Exception("No config file found.");

            if (fileNames.Length == takeCount)
                throw new Exception("Multiple config files found. Only one expected.");

            var configFileName = fileNames[0];
            return configFileName;
        }

        private IEnumerable<Tuple<string, string>> EnumerateConnectionStrings()
        {
            const string fileName = "connectionstrings.txt";

            string fullFileName = Path.Combine(_configSettingsDir, fileName);

            if (!File.Exists(fullFileName))
                throw new FileNotFoundException($"File not found in config settings directory: {fileName}");

            return
                from line in File.ReadLines(fullFileName, Encoding.UTF8)
                let index = line.IndexOf(' ')
                where index > -1
                let name = line.Substring(0, index)
                let connstring = line.Substring(index + 1)
                where !(string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(connstring))
                select Tuple.Create(name, connstring);
        }
    }
}
