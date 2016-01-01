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
        public string OutputDir { get; set; }
        public string ProjectName { get; set; }
        public string Configuration { get; set; }
    }
}
