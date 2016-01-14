using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDeploy
{
    public sealed class SquirrelRelease
    {
        public string SHA1 { get; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public string AppName { get; }
        public bool IsDelta { get; }

        public string FullVersion { get; }
        public int VersionMajorPart { get; set; }
        public string VersionDatePart { get; }
        public int VersionIncrementPart { get; }

        public SquirrelRelease(string SHA1, string fileName, long size)
        {
            this.SHA1 = SHA1;
            this.FileName = fileName;
            this.Size = size;

            var fileNameParts = FileName.Split('-');
            AppName = fileNameParts[0];
            FullVersion = fileNameParts[1];
            IsDelta = fileNameParts[2].StartsWith("delta", StringComparison.OrdinalIgnoreCase);

            var versionParts = FullVersion.Split('.');

            if (versionParts.Length != 3)
                throw new Exception($"Version is invalid: {FullVersion}");

            VersionMajorPart = int.Parse(versionParts[0]);
            VersionDatePart = versionParts[1];
            VersionIncrementPart = int.Parse(versionParts[2]);
        }
    }
}
