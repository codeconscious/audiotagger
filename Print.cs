using System;

namespace AudioTagger
{
    public static class Print
    {
        const string _separator = ":"; // Make into a user setting?
        const sbyte _minTitleWidth = -12; // Make into a user setting?

        private static void PrependLines(byte lines)
        {
            for (var prepend = 0; prepend < lines; prepend++)
                Console.WriteLine();
        }

        private static void AppendLines(byte lines)
        {
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
        }

        public static void Error(string message) =>
            Message(message, 1, 1, "ERROR: ");        

        public static void FileData(FileData fileData, string prependLine = "  • ", byte prepend = 0, byte append = 0)
        {
            PrependLines(prepend);

            var header = $"\"{fileData.FileName}\"";
            Console.WriteLine(header);

            // JA characters are wider than EN, so the alignment is off.
            Console.WriteLine(new string('-', header.Length));

            // TODO: Make labels multilingual
            TagDataWithHeader("Title", fileData.Title, prependLine);
            TagDataWithHeader("Artist(s)", string.Join(", ", fileData.Artists), prependLine);
            TagDataWithHeader("Album", fileData.Album, prependLine);
            TagDataWithHeader("Year", fileData.Year.ToString(), prependLine);
            TagDataWithHeader("Duration", fileData.Duration.ToString("m\\:ss"), prependLine);
            TagDataWithHeader("Genres", string.Join(", ", fileData.Genres), prependLine);
            TagDataWithHeader("Bitrate", fileData.BitRate.ToString(), prependLine);
            TagDataWithHeader("Sample Rate", fileData.SampleRate.ToString("#,##0"), prependLine);
            TagDataWithHeader("ReplayGain?", fileData.HasReplayGainData ? "Yes" : "No", prependLine);
            //PrintFormattedLine("Comment", fileData.Comments?.Substring(0, (fileData.Comments.Length > 70 ? 70 : fileData.Comments.Length-1)) ?? "N/A");

            if (fileData.Composers?.Length > 0)
                TagDataWithHeader($"Composers", string.Join("; ", fileData.Composers), prependLine);

            AppendLines(append);

            // I just wanted to practice using a local method.
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
