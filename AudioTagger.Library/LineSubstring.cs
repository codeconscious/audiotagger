using System;

namespace AudioTagger
{
    // Perhaps a good candidate for a record?
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
    }
}
