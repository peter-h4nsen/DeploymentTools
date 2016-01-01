using System;
using System.IO;
using System.Linq;

namespace SquirrelDeploy
{
    public sealed class SquirrelService
    {
        private string squirrelExePath;
        private readonly MiscFunctions _miscFunctions;

        public SquirrelService(MiscFunctions miscFunctions, string sourceDir)
        {
            if (miscFunctions == null)
                throw new ArgumentNullException(nameof(miscFunctions));

            if (!Directory.Exists(sourceDir))
                throw new ArgumentException($"Source directory not found: {sourceDir}");

            _miscFunctions = miscFunctions;

            SetExeFilePath(sourceDir);
        }

        public void Releasify(string nupkgFilePath, string releaseDir)
        {
            var args = $@"--releasify={nupkgFilePath} --releaseDir={releaseDir} --no-msi";
            _miscFunctions.RunProcess(squirrelExePath, args);
        }

        private void SetExeFilePath(string sourceDir)
        {
            const string exeName = "Squirrel.exe";

            var result = (
                from dir in Directory.EnumerateDirectories(sourceDir, "squirrel.windows.*", SearchOption.AllDirectories)
                let exeFilePath = Path.Combine(dir, "tools", exeName)
                where File.Exists(exeFilePath)
                select exeFilePath).FirstOrDefault();

            if (result == null)
            {
                throw new FileNotFoundException($"{exeName} not found. Have NuGet packages been restored?");
            }

            squirrelExePath = result;
        }
    }
}
