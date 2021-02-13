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

            for (var i = 0; i < args.Length; i++)
            {
                var filename = args[i]?.Trim();

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
                    taggedFile.Tag.Composers,
                    taggedFile.Tag.ReplayGainTrackGain > 0 || taggedFile.Tag.ReplayGainAlbumGain > 0);

                printer.PrintData(fileRecord);                
            }

            Console.WriteLine();
        }
    }
}
