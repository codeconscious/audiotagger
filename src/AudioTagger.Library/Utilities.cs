namespace AudioTagger.Library;

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

    public static string Pluralize(int count, string whenOne, string whenZeroOrMultiple) =>
        count switch
        {
            1 => whenOne,
            _ => whenZeroOrMultiple
        };
}
