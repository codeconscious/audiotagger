namespace AudioTagger
{
    public class LineSubString
    {
        public string Text { get; set; }
        public ConsoleColor? FgColor { get; set; }
        public ConsoleColor? BgColor { get; set; }

        public LineSubString(string text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
        {
            Text = text;
            FgColor = fgColor;
            BgColor = bgColor;
        }

        public string GetSpecterString()
        {
            var sb = new System.Text.StringBuilder();

            if (FgColor.HasValue)
            {
                sb.Append("[" + Utilities.ConvertToSpectreColor(FgColor.Value));

                if (BgColor.HasValue)
                    sb.Append(" on " + Utilities.ConvertToSpectreColor(BgColor.Value));

                sb.Append(']');
            }

            sb.Append(Text.Replace("[", "[[").Replace("]", "]]"));

            if (FgColor.HasValue)
                sb.Append("[/]");

            return sb.ToString();

        }
    }
}
