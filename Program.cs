using System;
using TagLib;

namespace Music
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
                    taggedFile.Properties.AudioBitrate,
                    taggedFile.Properties.AudioSampleRate,
                    taggedFile.Tag.Composers);

                var printer = new DataPrinter(1, "   • ", 1);

                printer.PrintData(fileRecord);
            }
        }
    }

    public record FileData(
        string Title, string[] Artists, TimeSpan Duration, string[] Genres,
        int BitRate, int SampleRate, string[] Composers);    

    public class DataPrinter // TODO: Make an interface to allow for different methods
    {
        private byte LinesToPrepend { get; set; }
        private string LineTextToPrepend { get; set; }
        private byte LinesToAppend { get; set; }

        public DataPrinter(byte linesToPrepend, string lineTextToPrepend, byte linesToAppend)
        {
            LinesToPrepend = linesToPrepend;
            LineTextToPrepend = lineTextToPrepend;
            LinesToAppend = linesToAppend;
        }

        public void PrintData(FileData fileData)
        {
            for (var prepend = 0; prepend < LinesToPrepend; prepend++)
                Console.WriteLine();

            Console.WriteLine(LineTextToPrepend + $"Title: {fileData.Title}");
            Console.WriteLine(LineTextToPrepend + $"Artist(s): {string.Join(", ", fileData.Artists)}");
            Console.WriteLine(LineTextToPrepend + $"Duration: {fileData.Duration:c}");
            Console.WriteLine(LineTextToPrepend + $"Genre: {string.Join(", ", fileData.Genres)}");
            Console.WriteLine(LineTextToPrepend + $"Bitrate: {fileData.BitRate}");
            Console.WriteLine(LineTextToPrepend + $"Sample Rate: {fileData.SampleRate:#,##0}");

            if (fileData.Composers?.Length > 0)
                Console.WriteLine(LineTextToPrepend + $"Composers: {string.Join("; ", fileData.Composers)}");

            for (var append = 0; append < LinesToAppend; append++)
                Console.WriteLine();
        }
    }
}
