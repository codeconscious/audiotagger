using static System.Console;
using Spectre.Console;

namespace AudioTagger.Console;

public class SpectrePrinter : IPrinter
{
    private static void PrependLines(byte lines)
    {
        if (lines == 0)
            return;

        for (var prepend = 0; prepend < lines; prepend++)
            WriteLine();
    }

    private static void AppendLines(byte lines)
    {
        if (lines == 0)
            return;

        for (var append = 0; append < lines; append++)
            WriteLine();
    }

    public void Print(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "",
                      ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message), "Message cannot be empty");

        PrependLines(prependLines);

        var subString = new LineSubString(message, fgColor, bgColor);
        AnsiConsole.MarkupLine(subString.GetSpectreString());

        AppendLines(appendLines);
    }

    public void Print(string message, ResultType type, byte prependLines = 0,
                      byte appendLines = 0)
    {
        Print(message, prependLines, appendLines, GetResultSymbol(type) + " ",
              ResultsMap.Map[type].Color, null);
    }

    public void Print(IEnumerable<LineSubString> lineParts,
                      byte prependLines = 0, byte appendLines = 1)
    {
        PrependLines(prependLines);

        if (!lineParts.Any())
            return;

        foreach (var linePart in lineParts)
        {
            AnsiConsole.Markup(
                new LineSubString(linePart.Text, linePart.FgColor, linePart.BgColor)
                    .GetSpectreString());
        }

        AppendLines(appendLines);
    }

    public void Print(IEnumerable<OutputLine> lines,
                      byte prependLines = 0, byte appendLines = 1)
    {
        PrependLines(prependLines);

        if (!lines.Any())
            return; // TODO: Think about this.

        foreach (var line in lines)
        {
            var lastLine = line.Line.Last();
            foreach (var lineParts in line.Line)
            {
                AnsiConsole.Markup(lineParts.GetSpectreString());

                if (lineParts == lastLine)
                    WriteLine();
            }
        }

        AppendLines(appendLines);
    }

    public void Error(string message) => Print(message, 1, 1, "ERROR: ");

    // private void PrintColor(LineSubString lineSubString)
    // {
    //     PrintColor(lineSubString.Text, lineSubString.FgColor, lineSubString.BgColor);
    // }

    // private void PrintColor(string text, ConsoleColor? fgColor,
    //                                ConsoleColor? bgColor = null,
    //                                bool addLineBreak = false)
    // {
    //     if (fgColor.HasValue)
    //         System.Console.ForegroundColor = fgColor.Value;

    //     if (bgColor.HasValue)
    //         BackgroundColor = bgColor.Value;

    //     Write(text);

    //     if (addLineBreak)
    //         WriteLine();

    //     ResetColor();
    // }

    // // TODO: Check whether we can delete this.
    // private void PrintColor()
    // {
    //     WriteLine();
    // }

    public char GetResultSymbol(ResultType type)
    {
        return type switch
        {
            ResultType.Cancelled => '×',
            ResultType.Failure => '×',
            ResultType.Neutral => '-',
            ResultType.Success => '◯',
            _ => '?'
        };
    }

    public void PrintTagDataToTable(MediaFile mediaFile, IDictionary<string, string> proposedUpdates)
    {
        ArgumentNullException.ThrowIfNull(proposedUpdates);

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn($"[aqua]{mediaFile.FileNameOnly.EscapeMarkup()}[/]");

        var tagTable = new Table();
        tagTable.Border(TableBorder.None);
        tagTable.AddColumns("Tag Name", "Tag Value"); // Hidden on the next line, though.
        tagTable.ShowHeaders = false;

        foreach (var line in OutputLine.GetTagKeyValuePairs(mediaFile))
        {
            tagTable.AddRow(line.Key, line.Value.EscapeMarkup());
        }

        table.AddRow(tagTable);
        tagTable.AddEmptyRow();

        foreach (var update in proposedUpdates)
        {
            tagTable.AddRow($"[olive]{update.Key}[/]", $"[yellow]{update.Value.EscapeMarkup()}[/]");
        }

        AnsiConsole.Write(table);
    }

    public void PrintException(Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }
}
