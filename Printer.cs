using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace AudioTagger
{
    public static class Printer
    {
        //private static bool _previousWasBlankLine;

        private static void PrependLines(byte lines)
        {
            if (lines == 0)
                return;

            //if (_previousWasBlankLine)
            //    lines--;

            for (var prepend = 0; prepend < lines; prepend++)
                Console.WriteLine();
        }

        private static void AppendLines(byte lines)
        {
            if (lines == 0)
                return;

            //_previousWasBlankLine = true;

            for (var append = 0; append < lines; append++)
                Console.WriteLine();
        }

        public static void Print(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "",
                                 ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Message cannot be empty");

            PrependLines(prependLines);
            PrintColor(prependText + message, fgColor, bgColor, true);
            AppendLines(appendLines);

            // if (appendLines > 0 || string.IsNullOrWhiteSpace(prependText + message))
            //     _previousWasBlankLine = true;
        }

        public static void Print(string message, ConsoleColor fgColor, ConsoleColor? bgColor = null)
        {
            Print(message, 0, 0, "", fgColor, bgColor);
        }

        public static void Print(IList<LineSubString> lineParts, byte prependLines = 0, byte appendLines = 1)
        {
            PrependLines(prependLines);

            if (!lineParts.Any())
                return;

            foreach (var linePart in lineParts)
                PrintColor(linePart.Text, linePart.FgColor, linePart.BgColor);

            AppendLines(appendLines);
        }

        public static void Print(IList<OutputLine> lines, byte prependLines = 0, byte appendLines = 1)
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

        public static void Print(string message, ResultType type, byte prependLines = 0,
                                 byte appendLines = 0, string prependText = "")
        {
            Print(message, prependLines, appendLines, prependText,
                  ResultsMap.Map[type].Color, null);
        }

        public static void Error(string message) =>
            Print(message, 1, 1, "ERROR: ");

        private static void PrintColor(LineSubString lineParts)
        {
            PrintColor(lineParts.Text, lineParts.FgColor, lineParts.BgColor);
        }

        private static void PrintColor(string text, ConsoleColor? fgColor, ConsoleColor? bgColor = null, bool addLineBreak = false)
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

        private static void PrintColor()
        {
            Console.WriteLine();
        }

        public static OutputLine TagDataWithHeader(string tagName, IList<LineSubString> tagData, string prependLine = "", ConsoleColor headerColor = ConsoleColor.DarkGray)
        {
            var spacesToPrepend = 4;
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

        public static OutputLine TagDataWithHeader(string tagName, string tagData, string prependLine = "", ConsoleColor headerColor = ConsoleColor.DarkGray)
        {
            return TagDataWithHeader(tagName, new List<LineSubString> { new LineSubString(tagData) }, prependLine, headerColor);
        }

        //private static void PrintColor(string text, bool addLineBreak = false)
        //{
        //    PrintColor(text, null, null, addLineBreak);
        //}        

        // TODO: Complete or delete
        //public static void UpdateData(UpdateableFields updates)
        //{
        //}

        public static char GetResultSymbol(ResultType type)
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
    }
}
