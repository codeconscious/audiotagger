﻿using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public interface IPrinter
    {
        void Print(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "",
                   ConsoleColor? fgColor = null, ConsoleColor? bgColor = null);

        void Print(IEnumerable<LineSubString> lineParts, byte prependLines = 0, byte appendLines = 1);

        void Print(IEnumerable<OutputLine> lines, byte prependLines = 0, byte appendLines = 1);

        void Print(string message, ResultType type, byte prependLines = 0, byte appendLines = 0);

        void Error(string message);

        char GetResultSymbol(ResultType type);

        void PrintDivider(string? text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null);
    }
}