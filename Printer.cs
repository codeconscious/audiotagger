using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace AudioTagger
{
    public static class Printer
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

        public static void Print(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "",
                                 ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Argument cannot be empty");

            PrependLines(prependLines);
            PrintColor(prependText + message, fgColor, bgColor, true);
            AppendLines(appendLines);

            // if (appendLines > 0 || string.IsNullOrWhiteSpace(prependText + message))
            //     _previousWasBlankLine = true;
        }

        public static void Print(IList<LineSubString> lineParts)
        {
            if (!lineParts.Any())
                return;

            foreach (var linePart in lineParts)
            {
                PrintColor(linePart.Text, linePart.FgColor, linePart.BgColor);
            }

            PrintColor();
        }

        public static void Print(LineOutputCollection lines)
        {
            var collection = lines.Lines;

            if (!collection.Any())
                return; // TODO: Think about this.

            foreach (var line in collection)
            {
                var lastLine = line.Line.Last();
                foreach (var lineParts in line.Line)
                {
                    PrintColor(lineParts);
                    if (lineParts == lastLine)
                        PrintColor();
                }
            }

            PrintColor();
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

        //private static void PrintColor(string text, bool addLineBreak = false)
        //{
        //    PrintColor(text, null, null, addLineBreak);
        //}        

        // TODO: Complete or delete
        //public static void UpdateData(UpdateableFields updates)
        //{
        //}
    }
}
