using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace AudioTagger
{
    public class ConsolePrinter : IPrinter
    {
        //private static bool _previousWasBlankLine;

        private void PrependLines(byte lines)
        {
            if (lines == 0)
                return;

            for (var prepend = 0; prepend < lines; prepend++)
                Console.WriteLine();
        }

        private void AppendLines(byte lines)
        {
            if (lines == 0)
                return;

            for (var append = 0; append < lines; append++)
                Console.WriteLine();
        }

        public void Print(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "",
                                 ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Message cannot be empty");

            PrependLines(prependLines);
            PrintColor(prependText + message, fgColor, bgColor, true);
            AppendLines(appendLines);
        }

        public void Print(IReadOnlyList<LineSubString> lineParts, byte prependLines = 0, byte appendLines = 1)
        {
            PrependLines(prependLines);

            if (!lineParts.Any())
                return;

            foreach (var linePart in lineParts)
                PrintColor(linePart.Text, linePart.FgColor, linePart.BgColor);

            AppendLines(appendLines);
        }

        public void Print(IList<OutputLine> lines, byte prependLines = 0, byte appendLines = 1)
        {
            PrependLines(prependLines);

            if (!lines.Any())
                return; // TODO: Think about this.

            foreach (var line in lines)
            {
                var lastLine = line.Line.Last();
                foreach (var lineParts in line.Line)
                {
                    PrintColor(lineParts);
                    if (lineParts == lastLine)
                        PrintColor();
                }
            }

            AppendLines(appendLines);
        }

        public void Print(string message, ResultType type, byte prependLines = 0,
                                 byte appendLines = 0)
        {
            Print(message, prependLines, appendLines, GetResultSymbol(type) + " ",
                  ResultsMap.Map[type].Color, null);
        }

        public void Error(string message) =>
            Print(message, 1, 1, "ERROR: ");

        private void PrintColor(LineSubString lineParts)
        {
            PrintColor(lineParts.Text, lineParts.FgColor, lineParts.BgColor);
        }

        private void PrintColor(string text, ConsoleColor? fgColor,
                                       ConsoleColor? bgColor = null,
                                       bool addLineBreak = false)
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

        private void PrintColor()
        {
            Console.WriteLine();
        }



        //private static void PrintColor(string text, bool addLineBreak = false)
        //{
        //    PrintColor(text, null, null, addLineBreak);
        //}

        // TODO: Complete or delete
        //public static void UpdateData(UpdateableFields updates)
        //{
        //}

        public char GetResultSymbol(ResultType type)
        {
            return type switch
            {
                ResultType.Cancelled => '×',
                ResultType.Failure => '×',
                ResultType.Neutral => '-',
                ResultType.Success => '◯',
                _ => '?'
            };
        }

        public IList<OutputLine> GetTagPrintedLines(MediaFile fileData)
        {
            var lines = new List<OutputLine>();

            var fileNameBase = System.IO.Path.GetFileNameWithoutExtension(fileData.Path);
            var fileNameExt = System.IO.Path.GetExtension(fileData.Path);
            lines.Add(
                new OutputLine(
                    new LineSubString(fileNameBase, ConsoleColor.Cyan),
                    new LineSubString(fileNameExt, ConsoleColor.DarkCyan)));

            lines.Add(OutputLine.TagDataWithHeader("Title", fileData.Title));
            lines.Add(OutputLine.TagDataWithHeader("Artist(s)", string.Join(", ", fileData.Artists)));
            lines.Add(OutputLine.TagDataWithHeader("Album", fileData.Album));
            lines.Add(OutputLine.TagDataWithHeader("Year", fileData.Year.ToString()));
            lines.Add(OutputLine.TagDataWithHeader("Duration", fileData.Duration.ToString("m\\:ss")));

            var genreCount = fileData.Genres.Length;
            lines.Add(OutputLine.TagDataWithHeader("Genre(s)", string.Join(", ", fileData.Genres) +
                                                (genreCount > 1 ? $" ({genreCount})" : "")));

            var bitrate = fileData.BitRate.ToString();
            var sampleRate = fileData.SampleRate.ToString("#,##0");
            var hasReplayGain = fileData.HasReplayGainData ? "ReplayGain OK" : "No ReplayGain";

            // Create formatted quality line
            const string genreSeparator = "    ";
            lines.Add(
                OutputLine.TagDataWithHeader(
                    "Quality",
                    new List<LineSubString>
                    {
                        new LineSubString(bitrate),
                        new LineSubString(" kbps" + genreSeparator, ConsoleColor.DarkGray),
                        new LineSubString(sampleRate),
                        new LineSubString(" kHz" + genreSeparator, ConsoleColor.DarkGray),
                        new LineSubString(hasReplayGain)
                    }));

            if (fileData.Composers?.Length > 0)
                lines.Add(OutputLine.TagDataWithHeader($"Composers", string.Join("; ", fileData.Composers)));

            if (!string.IsNullOrWhiteSpace(fileData.Comments))
                lines.Add(OutputLine.TagDataWithHeader("Comment", fileData.Comments));

            return lines;
        }
    }
}
