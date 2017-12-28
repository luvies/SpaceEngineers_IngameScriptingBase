using System;
using System.IO;
using System.Collections;

namespace Build
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings = Settings.LoadSettings();
            Console.WriteLine();

            if (!Directory.Exists(settings.FileInPath))
            {
                Console.WriteLine("File in path '{0}' not found, are you sure it exists? (relative directories allowed)", settings.FileInPath);
                Pause();
                return;
            }
            if (!Directory.Exists(settings.FileOutPath))
                Directory.CreateDirectory(settings.FileOutPath);

            Processor.Process(settings);

            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
