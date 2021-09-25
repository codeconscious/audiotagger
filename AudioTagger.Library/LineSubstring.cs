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
                sb.Append("[" + ConvertToSpectreColor(FgColor.Value));

                if (BgColor.HasValue)
                    sb.Append(" on " + ConvertToSpectreColor(BgColor.Value));

                sb.Append(']');
            }

            sb.Append(Text.Replace("[", "[[").Replace("]", "]]"));

            if (FgColor.HasValue)
                sb.Append("[/]");

            // Console.WriteLine("> " + sb.ToString());
            return sb.ToString();

        }

        private static string ConvertToSpectreColor(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Red => "red",
                ConsoleColor.Yellow => "yellow",
                ConsoleColor.Blue => "blue",
                ConsoleColor.Green => "green",
                ConsoleColor.Cyan => "cyan",
                ConsoleColor.Magenta => "magenta",
                ConsoleColor.Gray => "gray",
                _ => "white"
            };
        }
    }
}
