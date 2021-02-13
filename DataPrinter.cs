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
            PrintPrependLines();

            var header = $"\"{fileData.FileName}\"";
            Console.WriteLine(header);

            // JA characters are wider than EN, so the alignment is off.
            Console.WriteLine(new string('-', header.Length));

            // TODO: Make labels multilingual
            // TODO: Add label spaces so that data is aligned
            Console.WriteLine(LineTextToPrepend + $"Title       : {fileData.Title}");
            Console.WriteLine(LineTextToPrepend + $"Artist(s)   : {string.Join(", ", fileData.Artists)}");
            Console.WriteLine(LineTextToPrepend + $"Duration    : {fileData.Duration:m\\:ss}");
            Console.WriteLine(LineTextToPrepend + $"Genre       : {string.Join(", ", fileData.Genres)}");
            Console.WriteLine(LineTextToPrepend + $"Bitrate     : {fileData.BitRate}");
            Console.WriteLine(LineTextToPrepend + $"Sample Rate : {fileData.SampleRate:#,##0}");
            Console.WriteLine(LineTextToPrepend + $"ReplayGain? : {(fileData.HasReplayGainData ? "Yes" : "No")}");

            if (fileData.Composers?.Length > 0)
                Console.WriteLine(LineTextToPrepend + $"Composers: {string.Join("; ", fileData.Composers)}");

            PrintAppendLines();
        }

        public void PrintError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) // TODO: Turn on nullable references
                throw new ArgumentNullException(nameof(message), "Argument cannot be empty");

            PrintPrependLines();
            Console.WriteLine("ERROR: " + message);
            PrintAppendLines();            
        }

        private void PrintPrependLines()
        {
            for (var prepend = 0; prepend<LinesToPrepend; prepend++)
                Console.WriteLine();
        }

        private void PrintAppendLines()
        {
            for (var append = 0; append < LinesToAppend; append++)
                Console.WriteLine();
        }
    }
}
