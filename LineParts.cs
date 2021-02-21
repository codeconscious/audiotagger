using System;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    public class LineSubString
    {
        public string Text { get; set; }
        public ConsoleColor? FgColor { get; set; } = null;
        public ConsoleColor? BgColor { get; set; } = null;

        public LineSubString(string text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
        {
            Text = text;
            FgColor = fgColor;
            BgColor = bgColor;
        }

        //public LineParts(string text)
        //{
        //    Text = text;
        //}
    }

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

        public OutputLine(string text, ConsoleColor fgColor, ConsoleColor? bgColor = null)
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
    }
}
