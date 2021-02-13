using System;

namespace AudioTagger
{
    public class DataPrinter // TODO: Make an interface to allow for different methods
    {
        private byte LinesToPrepend { get; set; }
        private string LineTextToPrepend { get; set; }
        private byte LinesToAppend { get; set; }

        public DataPrinter(byte linesToPrepend, string lineTextToPrepend, byte linesToAppend = 0)
        {
            LinesToPrepend = linesToPrepend;
            LineTextToPrepend = lineTextToPrepend;
            LinesToAppend = linesToAppend;
        }

        public void PrintData(FileData fileData)
        {
            for (var prepend = 0; prepend < LinesToPrepend; prepend++)
                Console.WriteLine();

            Console.WriteLine($"\"{fileData.FileName}\"");

            // TODO: Make labels multilingual
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

        public void PrintError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Argument cannot be empty");

            // TODO: Violates DRY -- fix.
            for (var prepend = 0; prepend < LinesToPrepend; prepend++)
                Console.WriteLine();

            Console.WriteLine("ERROR: " + message);

            for (var append = 0; append < LinesToAppend; append++)
                Console.WriteLine();
        }
    }
}
