using System;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    public class OutputLine
    {
        public List<LineSubString> Line { get; set; } = new List<LineSubString>();

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
                                            string prependLine = "",
                                            ConsoleColor headerColor = ConsoleColor.DarkGray)
        {
            const int spacesToPrepend = 4;
            var spacesToAppend = 13 - tagName.Length; // TODO: Calculate this instead
            //var separator = ": ";

            var lineOutput = new OutputLine();

            lineOutput.Add(prependLine);
            lineOutput.Add(new string(' ', spacesToPrepend));
            lineOutput.Add(tagName, headerColor);
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
                prependLine,
                headerColor);
        }
    }
}
