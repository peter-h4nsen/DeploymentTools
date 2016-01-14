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
        private readonly string _filePath;

        public Logger(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            _filePath = filePath;
        }

        public void Write(string logMessage)
        {
            logMessage = $"{ DateTime.Now.ToString()}: {logMessage}";
            File.AppendAllLines(_filePath, new[] { logMessage });
            Console.WriteLine(logMessage);
        }

        public void DestroyExisting()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
