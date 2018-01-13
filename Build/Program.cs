using System;
using System.IO;
using System.Linq;

namespace Build
{
    class Program
    {
        static void Main(string[] args)
        {
            // set current directory to EXE path
            Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            // load settings
            Settings settings = Settings.LoadSettings();
            Console.WriteLine();

            // check if IN dir exists
            if (!Directory.Exists(settings.FileInPath))
            {
                Console.WriteLine("File in path '{0}' not found, are you sure it exists? (relative directories allowed)", settings.FileInPath);
                Pause();
                return;
            }
            // create OUT dir if it doesn't exist
            if (!Directory.Exists(settings.FileOutPath))
                Directory.CreateDirectory(settings.FileOutPath);

            // process files
            Processor.Process(settings);

#if DEBUG
            Pause();
#endif
        }

        private static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
