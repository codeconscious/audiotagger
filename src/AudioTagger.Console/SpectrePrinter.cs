using static System.Console;
using Spectre.Console;
using AudioTagger.Library;

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
        {
            return;
        }

        Write(string.Concat(Enumerable.Repeat(Environment.NewLine, count - 1)));
    }

    public void Print(string message, byte prependLines = 0, byte appendLines = 0,
                      string prependText = "", ConsoleColor? fgColor = null,
                      ConsoleColor? bgColor = null, bool addLinebreak = true)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentNullException(nameof(message), "Message cannot be empty");
        }

        PrintEmptyLines(prependLines);

        LineSubString subString = new(message, fgColor, bgColor);

        if (addLinebreak)
            AnsiConsole.MarkupLine(subString.GetSpectreString());
        else
            AnsiConsole.Markup(subString.GetSpectreString());

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

        if (lineParts.None())
        {
            return;
        }

        foreach (LineSubString linePart in lineParts)
        {
            var subString = new LineSubString(linePart.Text, linePart.FgColor, linePart.BgColor).GetSpectreString();
            if (linePart.AddLineBreak)
                AnsiConsole.MarkupLine(subString);
            else
                AnsiConsole.Markup(subString);
        };

        PrintEmptyLines(appendLines);
    }

    public void Print(IEnumerable<OutputLine> lines,
                      byte prependLines = 0, byte appendLines = 1)
    {
        PrintEmptyLines(prependLines);

        if (lines.None())
        {
            return;
        }

        foreach (OutputLine line in lines)
        {
            LineSubString lastLine = line.Line.Last();
            foreach (LineSubString lineParts in line.Line)
            {
                AnsiConsole.Markup(lineParts.GetSpectreString());

                if (lineParts == lastLine)
                    WriteLine();
            }
        }

        PrintEmptyLines(appendLines);
    }

    public void FirstError(IResultBase failResult, string? prepend = null)
    {
        string pre = prepend is null ? string.Empty : $"{prepend} ";
        string message = failResult?.Errors?.FirstOrDefault()?.Message ?? string.Empty;

        Error($"{pre}{message}");
    }

    public void Error(string message) =>
        Print(message, prependText: "ERROR: ", fgColor: ConsoleColor.Red);

    public void Warning(string message) => Print(message, fgColor: ConsoleColor.Yellow);

    public void Success(string message) => Print(message, fgColor: ConsoleColor.Green);

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

    public void PrintTagDataToTable(MediaFile mediaFile, IDictionary<string, string> proposedUpdates, bool includeComments)
    {
        ArgumentNullException.ThrowIfNull(proposedUpdates);

        Table table = new();
        table.Border(TableBorder.Rounded);
        table.AddColumn($"[aqua]{mediaFile.FileNameOnly.EscapeMarkup()}[/]");

        Table tagTable = new();
        tagTable.Border(TableBorder.None);
        tagTable.AddColumns("Tag Name", "Tag Value"); // Hidden on the next line, though.
        tagTable.ShowHeaders = false;

        foreach (KeyValuePair<string, string> line in
                 OutputLine.GetTagKeyValuePairs(mediaFile, includeComments))
        {
            tagTable.AddRow(line.Key, line.Value.EscapeMarkup());
        }

        table.AddRow(tagTable);
        tagTable.AddEmptyRow();

        foreach (KeyValuePair<string, string> update in proposedUpdates)
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
