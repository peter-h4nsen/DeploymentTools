using System;
using System.IO;

namespace SquirrelDeploy
{
    class Program
    {
        static int Main(string[] args)
        {
            var exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            SetWorkingDirectory(exeLocation);

            var logger = new Logger(GetLoggerFilename(exeLocation));
            var miscFunctions = new MiscFunctions();
            var nugetService = new NugetService(miscFunctions);

            try
            {
                logger.DestroyExisting();

                Args arguments = new ArgumentParser().Parse<Args>(args);

                var squirrelService = new SquirrelService(miscFunctions, arguments.SourceDir);
                SquirrelRelease latestRelease = squirrelService.FindLatestRelease(arguments.ReleaseDir);

                string nupkgFilePath = nugetService.GeneratePackage(
                    arguments.SourceDir, arguments.ProjectName, arguments.AppName, 
                    arguments.Author, arguments.Configuration, arguments.MajorVersion, latestRelease);

                squirrelService.Releasify(nupkgFilePath, arguments.ReleaseDir);

                return 0;
            }
            catch (Exception ex)
            {
                logger.Write(ex.Message);
                return 1;
            }
            finally
            {
                nugetService.CleanUp();
            }
        }

        private static void SetWorkingDirectory(string exeLocation)
        {
            var exeDirectory = Path.GetDirectoryName(exeLocation);
            Environment.CurrentDirectory = exeDirectory;
        }

        private static string GetLoggerFilename(string exeLocation)
        {
            var exeName = Path.GetFileNameWithoutExtension(exeLocation);
            return $"{exeName}_errorlog.txt";
        }
    }
}
