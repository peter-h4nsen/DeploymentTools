using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDeploy
{
    public sealed class MiscFunctions
    {
        public void RunProcess(string fileName, string args)
        {
            var psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.FileName = fileName;
            psi.Arguments = args;

            var process = Process.Start(psi);
            process.WaitForExit();
        }
    }
}
