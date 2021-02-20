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

    public class LineOutput
    {
        public List<LineSubString> Line { get; set; } = new List<LineSubString>();

        public LineOutput()
        {
            //Line.Add(lineParts);
        }

        public LineOutput(LineSubString lineParts)
        {
            Line = new List<LineSubString> { lineParts };
        }

        public LineOutput(params LineSubString[] lineParts)
        {
            Line = lineParts.ToList();
        }

        public LineOutput(string text, ConsoleColor fgColor, ConsoleColor? bgColor = null)
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

    public class LineOutputCollection
    {
        public List<LineOutput> Lines { get; set; } = new List<LineOutput>();
        
        public LineOutputCollection()
        {

        }

        public LineOutputCollection(LineSubString lineParts)
        {
            Lines.Add(new LineOutput(lineParts));
        }

        public void Add(LineOutput lineOutput)
        {
            Lines.Add(lineOutput);
        }

        public void Add(LineSubString lineParts)
        {
            Lines.Add(new LineOutput(lineParts));
        }

        public void Add(string text, ConsoleColor fgColor, ConsoleColor? bgColor = null)
        {
            Lines.Add(new LineOutput(text, fgColor, bgColor));
        }
    }
}
