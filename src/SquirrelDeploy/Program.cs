using System;
using System.IO;

namespace SquirrelDeploy
{
    class Program
    {
        static int Main(string[] args)
        {
            SetWorkingDirectory();

            var logger = new Logger();
            var miscFunctions = new MiscFunctions();
            var nugetService = new NugetService(miscFunctions);

            try
            {
                logger.DestroyExisting();

                Args arguments = new ArgumentParser().Parse<Args>(args);
                var squirrelService = new SquirrelService(miscFunctions, arguments.SourceDir);

                var nupkgFilePath = nugetService.GeneratePackage(
                    arguments.SourceDir, arguments.ProjectName, arguments.Configuration);

                squirrelService.Releasify(nupkgFilePath, arguments.OutputDir);

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

        private static void SetWorkingDirectory()
        {
            var exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = exeDirectory;
        }
    }
}
