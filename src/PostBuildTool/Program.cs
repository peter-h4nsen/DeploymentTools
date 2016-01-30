using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PostBuildTool
{
    class Program
    {
        static int Main(string[] args)
        {
           var arguments = new ArgumentParser().Parse<Args>(args);

            if (!Directory.Exists(arguments.AppDir))
                throw new Exception("App directory not found.");

            if (!Directory.Exists(arguments.ConfigSettingsDir))
                throw new Exception("Config settings directory not found.");

            var configEditor = new ConfigEditor(
                arguments.AppDir, arguments.ConfigSettingsDir);

            configEditor.SetConnectionStrings();

            Console.WriteLine("PostBuildTool was run successfully.");
            return 0;
        }
    }
}
