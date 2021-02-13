using System;
using System.IO;

namespace AudioTagger
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();

            var printer = new DataPrinter(1, "   • ");

            foreach (var filename in args)
            {
                if (string.IsNullOrWhiteSpace(filename))
                {
                    printer.PrintError("No filename was entered.");
                    continue;
                }

                if (!File.Exists(filename))
                {
                    printer.PrintError($"File \"{filename}\" was not found.");
                    continue;
                }

                var taggedFile = TagLib.File.Create(filename);

                var fileRecord = new FileData(
                    Path.GetFileName(filename),
                    taggedFile.Tag.Title,
                    taggedFile.Tag.Performers,
                    taggedFile.Properties.Duration,
                    taggedFile.Tag.Genres,
                    taggedFile.Properties.AudioBitrate, // TODO: Figure out why this is always 0
                    taggedFile.Properties.AudioSampleRate,
                    taggedFile.Tag.Composers);                

                printer.PrintData(fileRecord);                
            }

            Console.WriteLine();
        }
    }

        

    
}
