using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDeploy
{
    public sealed class Logger
    {
        private static readonly string filePath = @"errorlog.txt";

        public void Write(string logMessage)
        {
            logMessage = $"{ DateTime.Now.ToString()}: {logMessage}";
            File.AppendAllLines(filePath, new[] { logMessage });
            Console.WriteLine(logMessage);
        }

        public void DestroyExisting()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
