using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDeploy
{
    public class Args
    {
        public string SourceDir { get; set; }
        public string ReleaseDir { get; set; }
        public string ProjectName { get; set; }
        public string AppName { get; set; }
        public string Author { get; set; }
        public string Configuration { get; set; }
        public int MajorVersion { get; set; }
    }
}
