using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public interface IPrinter
    {
        void Print(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "",
                   ConsoleColor? fgColor = null, ConsoleColor? bgColor = null);

        void Print(IReadOnlyList<LineSubString> lineParts, byte prependLines = 0, byte appendLines = 1);

        void Print(IList<OutputLine> lines, byte prependLines = 0, byte appendLines = 1);

        void Print(string message, ResultType type, byte prependLines = 0, byte appendLines = 0);

        void Error(string message);

        char GetResultSymbol(ResultType type);

        IList<OutputLine> GetTagPrintedLines(MediaFile fileData);

        OutputLine TagDataWithHeader(string tagName, IReadOnlyList<LineSubString> tagData,
                                     string prependLine = "",
                                     ConsoleColor headerColor = ConsoleColor.DarkGray);

        OutputLine TagDataWithHeader(string tagName, string tagData,
                                     string prependLine = "",
                                     ConsoleColor headerColor = ConsoleColor.DarkGray);
    }
}
