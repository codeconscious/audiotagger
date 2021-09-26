namespace AudioTagger
{
    public static class Utilities
    {
        public static string ConvertToSpectreColor(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Red => "red",
                ConsoleColor.DarkRed => "maroon",
                ConsoleColor.Yellow => "yellow",
                ConsoleColor.Blue => "blue",
                ConsoleColor.Green => "green",
                ConsoleColor.Cyan => "aqua",
                ConsoleColor.DarkCyan => "teal",
                ConsoleColor.Magenta => "magenta",
                ConsoleColor.Gray => "silver",
                ConsoleColor.DarkGray => "grey",
                _ => "white"
            };
        }

        /// <summary>
        /// Sanitize a Spectre.Console markup string. (In Spectre, single brackets indicate
        /// the start of formatting commands, so regular brackets must be doubled to indicate that
        /// they are regular ones. The doubled brackets will ultimately be rendered as single ones.)
        /// </summary>
        /// <param name="text">A Spectre.Console markup string</param>
        /// <returns>A sanitized string.</returns>
        public static string SanitizeSpectreString(string? text)
        {
            return
                text?
                    .Replace("[", "[[")
                    .Replace("]", "]]")
                ?? "";
        }
    }
}