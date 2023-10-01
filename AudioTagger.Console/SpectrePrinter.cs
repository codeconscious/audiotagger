using static System.Console;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class SpectrePrinter : IPrinter
{
    /// <summary>
    /// Prints the requested number of blank lines.
    /// </summary>
    /// <param name="count"></param>
    private static void PrintEmptyLines(byte count)
    {
        if (count == 0)
            return;

        Write(string.Concat(Enumerable.Repeat(Environment.NewLine, count - 1)));
    }

    public void Print(string message, byte prependLines = 0, byte appendLines = 0,
                      string prependText = "", ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message), "Message cannot be empty");

        PrintEmptyLines(prependLines);

        var subString = new LineSubString(message, fgColor, bgColor);
        AnsiConsole.MarkupLine(subString.GetSpectreString());

        PrintEmptyLines(appendLines);
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
        PrintEmptyLines(prependLines);

        if (!lineParts.Any())
            return;

        lineParts.ToList().ForEach(p =>
        {
            if (p.AddLineBreak)
                AnsiConsole.MarkupLine(new LineSubString(p.Text, p.FgColor, p.BgColor).GetSpectreString());
            else
                AnsiConsole.Markup(new LineSubString(p.Text, p.FgColor, p.BgColor).GetSpectreString());
        });

        PrintEmptyLines(appendLines);
    }

    public void Print(IEnumerable<OutputLine> lines,
                      byte prependLines = 0, byte appendLines = 1)
    {
        PrintEmptyLines(prependLines);

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

        PrintEmptyLines(appendLines);
    }

    public void Error(string message) =>
        Print(message, prependText: "ERROR: ", fgColor: ConsoleColor.Red);

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
