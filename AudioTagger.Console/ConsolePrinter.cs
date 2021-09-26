using System;
using System.Linq;
using System.Collections.Generic;
using static System.Console;

namespace AudioTagger.Console
{
    public class ConsolePrinter : IPrinter
    {
        //private static bool _previousWasBlankLine;

        private void PrependLines(byte lines)
        {
            if (lines == 0)
                return;

            for (var prepend = 0; prepend < lines; prepend++)
                WriteLine();
        }

        private void AppendLines(byte lines)
        {
            if (lines == 0)
                return;

            for (var append = 0; append < lines; append++)
                WriteLine();
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
                System.Console.ForegroundColor = fgColor.Value;

            if (bgColor.HasValue)
                BackgroundColor = bgColor.Value;

            Write(text);

            if (addLineBreak)
                WriteLine();

            ResetColor();
        }

        // TODO: Check whether we can delete this.
        private void PrintColor()
        {
            WriteLine();
        }

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

        public void PrintDivider(string? text)
        {
            const char dividerChar = '-';

            var textLength = text?.Length ?? 0;
            var screenWidth = System.Console.WindowWidth;

            var diff = screenWidth - textLength;

            // If the text is wider than the screen, or close to it.
            if (diff <= 5)
            {
                WriteLine(new string(dividerChar, screenWidth - 1));

                if (textLength > 0)
                    WriteLine(text);
            }
            else
            {
                Write(new string(dividerChar, 2) + " ");
                Write(text);
                Write(" " + new string(dividerChar, diff - 4));
            }

            // WriteLine("");
        }
    }
}
