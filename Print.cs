using System;
using System.Text;
using System.Linq;

namespace AudioTagger
{
    public static class Print
    {
        private static bool _previousWasBlankLine;

        private static void PrependLines(byte lines)
        {
            if (lines == 0)
                return;

            if (_previousWasBlankLine)
                lines--;

            for (var prepend = 0; prepend < lines; prepend++)
                Console.WriteLine();
        }

        private static void AppendLines(byte lines)
        {
            if (lines == 0)
                return;

            _previousWasBlankLine = true;

            for (var append = 0; append < lines; append++)
                Console.WriteLine();
        }

        public static void Message(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "")
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Argument cannot be empty");

            PrependLines(prependLines);
            Console.WriteLine(prependText + message);
            AppendLines(appendLines);

            // if (appendLines > 0 || string.IsNullOrWhiteSpace(prependText + message))
            //     _previousWasBlankLine = true;
        }

        public static void Error(string message) =>
            Message(message, 1, 1, "ERROR: ");        

        // TODO: Move to the FileData class, probably.
        public static void FileData(FileData fileData, string prependLine = "  • ", byte prependLines = 0, byte appendLines = 0)
        {
            PrependLines(prependLines);

            var header = $"\"{fileData.FileName}\"";
            PrintText(header, ConsoleColor.DarkGreen, null, true);

            // JA characters are wider than EN, so the alignment is off.
            // TODO: Delete if not needed.
            // Console.WriteLine(new string('—', header.Length * 2));
            // var separator = new StringBuilder();
            // foreach (var ch in header)
            // {
            //     separator.Append(ch > 256 ? '―' : '–');
            // }
            // Console.WriteLine(separator.ToString());

            // TODO: Make labels multilingual?
            TagDataWithHeader("Title", fileData.Title, prependLine);
            TagDataWithHeader("Artist(s)", string.Join(", ", fileData.Artists), prependLine);
            TagDataWithHeader("Album", fileData.Album, prependLine);
            TagDataWithHeader("Year", fileData.Year.ToString(), prependLine);
            TagDataWithHeader("Duration", fileData.Duration.ToString("m\\:ss"), prependLine);
            TagDataWithHeader("Genre(s)", string.Join(", ", fileData.Genres), prependLine);

            var bitrate = fileData.BitRate.ToString();
            var sampleRate = fileData.SampleRate.ToString("#,##0");
            var hasReplayGain = fileData.HasReplayGainData ? "ReplayGain OK" : "No ReplayGain";
            //TagDataWithHeader("Quality", $"{bitrate}kbps {"|".Pastel(System.Drawing.Color.DimGray)} {sampleRate}kHz {"|".Pastel(System.Drawing.Color.DimGray)} {hasReplayGain}", prependLine);
            TagDataWithHeader("Quality", $"{bitrate}kbps | {sampleRate}kHz | {hasReplayGain}", prependLine);

            if (fileData.Composers?.Length > 0)
                TagDataWithHeader($"Composers", string.Join("; ", fileData.Composers), prependLine);

            if (!string.IsNullOrWhiteSpace(fileData.Comments))
                TagDataWithHeader("Comment", fileData.Comments, prependLine);

            AppendLines(appendLines);

            // I just wanted to practice using a local method...
            static void TagDataWithHeader(string tagName, string tagData, string toPrepend = "")
            {
                /* TODO: Remove Pastel-related code if the library will be removed. */
                //var formattedTitle = title/*.Pastel(System.Drawing.Color.DimGray)*/;
                var spacesToPrepend = 4;
                var spacesToAppend = 11 - tagName.Length;
                var separator = ": "/*.Pastel(System.Drawing.Color.DarkSlateGray)*/;

                PrintText(toPrepend);                
                PrintText(new string(' ', spacesToPrepend));
                PrintText(tagName, ConsoleColor.DarkGray);
                PrintText(new string(' ', spacesToAppend));
                PrintText(separator, ConsoleColor.DarkGray);
                PrintText(tagData, true);
            }
        }

        private static void PrintText(string text, ConsoleColor? fgColor, ConsoleColor? bgColor = null, bool addLineBreak = false)
        {
            if (fgColor.HasValue)
                Console.ForegroundColor = fgColor.Value;

            if (bgColor.HasValue)
                Console.BackgroundColor = bgColor.Value;

            Console.Write(text);

            if (addLineBreak)
                Console.WriteLine();

            Console.ResetColor();
        }

        private static void PrintText(string text, bool addLineBreak = false)
        {
            PrintText(text, null, null, addLineBreak);
        }

        // TODO: Complete or delete
        //public static void UpdateData(UpdateableFields updates)
        //{

        //}
    }
}
