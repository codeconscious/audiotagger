namespace AudioTagger;

public sealed class LineSubString
{
    public string Text { get; set; }
    public ConsoleColor? FgColor { get; set; }
    public ConsoleColor? BgColor { get; set; }
    public bool AddLineBreak { get; set; }

    public LineSubString(
        string text,
        ConsoleColor? fgColor = null,
        ConsoleColor? bgColor = null,
        bool addLineBreak = false)
    {
        Text = text;
        FgColor = fgColor;
        BgColor = bgColor;
        AddLineBreak = addLineBreak;
    }

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
