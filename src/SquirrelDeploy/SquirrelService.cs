using System;
using System.IO;
using System.Linq;
using System.Text;

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

        public SquirrelRelease FindLatestRelease(string releaseDir)
        {
            const string fileName = "RELEASES";

            var path = Path.Combine(releaseDir, fileName);

            // If file is not found this must be the initial release.
            if (!File.Exists(path))
            {
                return null;
            }

            // Find releases by reading the RELEASES file. Ordering by version will get the latest.
            var allReleases =
                from line in File.ReadLines(path, Encoding.UTF8)
                where !string.IsNullOrWhiteSpace(line)
                let release = ParseReleaseLine(line)
                where !release.IsDelta
                orderby release.VersionMajorPart descending, release.VersionDatePart descending, release.VersionIncrementPart descending
                select release;

            return allReleases.FirstOrDefault();
        }

        private SquirrelRelease ParseReleaseLine(string line)
        {
            const string errorText = "Can't parse release file";

            var lineParts = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (lineParts.Length != 3)
                throw new Exception($"{errorText}. Line seems to be invalid.");

            string SHA1 = lineParts[0];
            string fileName = lineParts[1];

            long size;

            if (!long.TryParse(lineParts[2], out size))
                throw new Exception($"{errorText}. 'Size' is not a number.");

            return new SquirrelRelease(SHA1, fileName, size);
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
