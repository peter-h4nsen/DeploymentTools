using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitBot
{
    class Program
    {
        private const string _workingDirectory = @"C:\Code\GitHub\SquirrelDemoApp\";
        private const string _codeFileName = @"c:\Code\GitHub\SquirrelDemoApp\SquirrelDemoApp\MainWindow.xaml";
        
        private static ManualResetEventSlim _exitWaitHandle = new ManualResetEventSlim(false);

        static void Main(string[] args)
        {
            var task = Task.Run(() => 
            {
                DateTime nextRunTime = GetNextRunTime();

                do
                {
                    if (DateTime.Now >= nextRunTime)
                    {
                        RunJob();
                        nextRunTime = GetNextRunTime();
                    }
                }
                while (!_exitWaitHandle.Wait(TimeSpan.FromSeconds(5)));
            });

            while (Console.ReadLine().ToLower() != "exit") ;

            Console.WriteLine("Waiting for task to finish before exitting..");
            _exitWaitHandle.Set();
            task.Wait();
        }

        private static void RunJob()
        {
            const string messagePrefix = "Message from GitBot:";

            var code = File.ReadAllText(_codeFileName, Encoding.UTF8);

            var changedCode = 
                Regex.Replace(code, $@"{messagePrefix}[^""]*", 
                $"{messagePrefix} Hi, I changed this text at [{DateTime.Now.ToString()}]");

            File.WriteAllText(_codeFileName, changedCode, Encoding.UTF8);

            RunGitCommand("commit -a -m \"Code changed by GitBot.\"");
            RunGitCommand("push");

            Console.WriteLine("New code was committed and pushed successfully.");
        }

        private static void RunGitCommand(string arguments)
        {
            var psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.FileName = "git";
            psi.Arguments = arguments;
            psi.WorkingDirectory = _workingDirectory;

            var process = Process.Start(psi);
            process.WaitForExit();

            string stderr = process.StandardError.ReadToEnd();
            string stdout = process.StandardOutput.ReadToEnd();
            
            process.Close();
        }

        private static DateTime GetNextRunTime()
        {
            var minutesToAdd = new Random().Next(10, 45);
            var nextRunTime = DateTime.Now.AddMinutes(minutesToAdd);
            Console.WriteLine($"Next job will run at: {nextRunTime.ToString()}");
            return nextRunTime;
        }
    }
}
