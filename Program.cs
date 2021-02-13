using System;
using TagLib;

namespace AudioTagger
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var filename in args)
            {
                if (string.IsNullOrWhiteSpace(filename))
                {
                    Console.WriteLine("A filename was not entered...");
                    continue;
                }

                var taggedFile = TagLib.File.Create(filename);

                var fileRecord = new FileData(
                    taggedFile.Tag.Title,
                    taggedFile.Tag.Performers,
                    taggedFile.Properties.Duration,
                    taggedFile.Tag.Genres,
                    taggedFile.Properties.AudioBitrate, // TODO: Figure out why this is a
                    taggedFile.Properties.AudioSampleRate,
                    taggedFile.Tag.Composers);

                var printer = new DataPrinter(1, "   • ", 1);

                printer.PrintData(fileRecord);
            }
        }
    }

        

    
}
