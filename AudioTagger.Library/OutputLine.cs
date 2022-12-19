using System;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    public class OutputLine
    {
        public List<LineSubString> Line { get; set; } = new List<LineSubString>();

        // TODO: Can we delete this?
        public OutputLine()
        {
            //Line.Add(lineParts);
        }

        public OutputLine(LineSubString lineParts)
        {
            Line = new List<LineSubString> { lineParts };
        }

        public OutputLine(params LineSubString[] lineParts)
        {
            Line = lineParts.ToList();
        }

        public OutputLine(string text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
        {
            Line.Add(new LineSubString(text, fgColor, bgColor));
        }

        public void Add(LineSubString lineParts)
        {
            Line.Add(lineParts);
        }

        public void Add(string text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
        {
            Line.Add(new LineSubString(text, fgColor, bgColor));
        }

        public static OutputLine TagDataWithHeader(string tagName, IReadOnlyList<LineSubString> tagData,
                                            string prependLine = "")
        {
            const int spacesToPrepend = 4;
            var spacesToAppend = 13 - tagName.Length; // TODO: Calculate this instead
            //var separator = ": ";

            var lineOutput = new OutputLine();

            lineOutput.Add(prependLine);
            lineOutput.Add(new string(' ', spacesToPrepend));
            lineOutput.Add(tagName);
            lineOutput.Add(new string(' ', spacesToAppend));

            foreach (var part in tagData)
                lineOutput.Add(part);

            return lineOutput;
        }

        public static OutputLine TagDataWithHeader(string tagName, string tagData,
                                            string prependLine = "",
                                            ConsoleColor headerColor = ConsoleColor.DarkGray)
        {
            return TagDataWithHeader(
                tagName,
                new List<LineSubString>
                {
                    new LineSubString(tagData)
                },
                prependLine);
        }

        public static IList<OutputLine> GetTagPrintedLines(MediaFile fileData)
        {
            var lines = new List<OutputLine>
            {
                TagDataWithHeader("Title", fileData.Title),
                TagDataWithHeader("Artist(s)", string.Join(", ", fileData.Artists)),
                TagDataWithHeader("Album", fileData.Album),
                TagDataWithHeader("Year", fileData.Year.ToString()),
                TagDataWithHeader("Duration", fileData.Duration.ToString("m\\:ss"))
            };

            var genreCount = fileData.Genres.Length;
            lines.Add(TagDataWithHeader("Genre(s)", string.Join(", ", fileData.Genres) +
                                                    (genreCount > 1 ? $" ({genreCount})" : "")));

            var bitrate = fileData.BitRate.ToString();
            var sampleRate = fileData.SampleRate.ToString("#,##0");
            var trackReplayGain = fileData.ReplayGainTrack.ToString();

            // Create formatted quality line
            const string genreSeparator = "    ";
            lines.Add(
                TagDataWithHeader(
                    "Quality",
                    new List<LineSubString>
                    {
                        new LineSubString(bitrate),
                        new LineSubString(" kbps" + genreSeparator, ConsoleColor.DarkGray),
                        new LineSubString(sampleRate),
                        new LineSubString(" kHz" + genreSeparator, ConsoleColor.DarkGray),
                        new LineSubString("RG: " + trackReplayGain)
                    }));

            if (fileData.Composers?.Length > 0)
                lines.Add(TagDataWithHeader($"Composers", string.Join("; ", fileData.Composers)));

            if (!string.IsNullOrWhiteSpace(fileData.Comments))
                lines.Add(TagDataWithHeader("Comment", fileData.Comments));

            return lines;
        }

        public static Dictionary<string, string> GetTagKeyValuePairs(MediaFile fileData)
        {
            var lines = new Dictionary<string, string>();

            lines.Add("Title", fileData.Title);
            lines.Add("Artist(s)", string.Join(", ", fileData.Artists));
            lines.Add("Album", fileData.Album);
            lines.Add("Year", fileData.Year.ToString());
            lines.Add("Duration", fileData.Duration.ToString("m\\:ss"));

            var genreCount = fileData.Genres.Length;
            lines.Add("Genre(s)", string.Join(", ", fileData.Genres) +
                                  (genreCount > 1 ? $" ({genreCount})" : ""));

            var bitrate = fileData.BitRate.ToString();
            var sampleRate = fileData.SampleRate.ToString("#,##0");
            var trackReplayGain = fileData.ReplayGainTrack.ToString();

            // Create formatted quality line
            const string separator = "  |  ";
            lines.Add(
                "Quality",
                $"{bitrate}kbps" + separator + $"{sampleRate} kHz" + separator + "RG: " + trackReplayGain);

            if (fileData.Composers?.Length > 0)
                lines.Add($"Composers", string.Join("; ", fileData.Composers));

            if (!string.IsNullOrWhiteSpace(fileData.Comments))
                lines.Add("Comment", fileData.Comments);

            return lines;
        }
    }
}
