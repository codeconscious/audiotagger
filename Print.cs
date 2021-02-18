using System;

namespace AudioTagger
{
    public static class Print
    {
        const string _separator = ":"; // Make into a user setting?
        const sbyte _minTitleWidth = -11; // Make into a user setting?
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
        public static void FileData(FileData fileData, string prependLine = "  • ", byte prepend = 0, byte append = 0)
        {
            PrependLines(prepend);

            var header = $"\"{fileData.FileName}\"";
            Console.WriteLine(header);

            // JA characters are wider than EN, so the alignment is off.
            Console.WriteLine(new string('–', header.Length));

            // TODO: Make labels multilingual
            TagDataWithHeader("Title", fileData.Title, prependLine);
            TagDataWithHeader("Artist(s)", string.Join(", ", fileData.Artists), prependLine);
            TagDataWithHeader("Album", fileData.Album, prependLine);
            TagDataWithHeader("Year", fileData.Year.ToString(), prependLine);
            TagDataWithHeader("Duration", fileData.Duration.ToString("m\\:ss"), prependLine);
            TagDataWithHeader("Genres", string.Join(", ", fileData.Genres), prependLine);

            var bitrate = fileData.BitRate.ToString();
            var sampleRate = fileData.SampleRate.ToString("#,##0");
            var hasReplayGain = fileData.HasReplayGainData ? "ReplayGain OK" : "No ReplayGain";
            TagDataWithHeader("Quality", $"{bitrate}kbps | {sampleRate}kHz | {hasReplayGain}", prependLine);
            
            if (fileData.Composers?.Length > 0)
                TagDataWithHeader($"Composers", string.Join("; ", fileData.Composers), prependLine);

            if (!string.IsNullOrWhiteSpace(fileData.Comments))
                TagDataWithHeader("Comment", fileData.Comments, prependLine);

            AppendLines(append);

            // I just wanted to practice using a local method...
            static void TagDataWithHeader(string title, string data, string prepend = "")
            {
                Console.Write(prepend);
                Console.Write(string.Format($"{title,_minTitleWidth}"));
                Console.WriteLine($"{_separator} {data}");
            }
        }

        public static void UpdateData(UpdateableFields updates)
        {

        }
    }
}
