namespace AudioTagger.Library;

public sealed class LineSubString(
    string text,
    ConsoleColor? fgColor = null,
    ConsoleColor? bgColor = null,
    bool addLineBreak = false)
{
    public string Text { get; set; } = text;
    public ConsoleColor? FgColor { get; set; } = fgColor;
    public ConsoleColor? BgColor { get; set; } = bgColor;
    public bool AddLineBreak { get; set; } = addLineBreak;

    public string GetSpectreString()
    {
        var sb = new System.Text.StringBuilder();

        if (FgColor.HasValue)
        {
            sb.Append('[').Append(Utilities.ConvertToSpectreColor(FgColor.Value));

            if (BgColor.HasValue)
                sb.Append(" on ").Append(Utilities.ConvertToSpectreColor(BgColor.Value));

            sb.Append(']');
        }

        sb.Append(Text.Replace("[", "[[").Replace("]", "]]"));

        if (FgColor.HasValue)
            sb.Append("[/]");

        return sb.ToString();
    }
}
