using Spectre.Console;

namespace AudioTagger.Console;

public sealed class TagViewerSummary : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        ArgumentNullException.ThrowIfNull(mediaFiles);

        var orderedFiles = mediaFiles.OrderBy(m => string.Concat(m.AlbumArtists) ?? string.Empty)
                                     .ThenBy(m => string.Concat(m.Artists) ?? string.Empty)
                                     .ThenBy(m => m.Album ?? string.Empty)
                                     .ThenBy(m => m.TrackNo)
                                     .ThenBy(m => m.Title)
                                     .ToImmutableArray();

        Table table = CreateTableWithColumns(
            ("Artist(s)", Justify.Left),
            ("Album", Justify.Left),
            ("Trk", Justify.Right),
            ("Title", Justify.Left),
            ("Year", Justify.Right),
            ("Genre(s)", Justify.Left),
            ("Dur.", Justify.Right),
            ("RG(Tr)", Justify.Right));

        table = AppendDataRowsToTable(table,
                                      orderedFiles,
                                      printer);

        AnsiConsole.Write(table);
    }

    private static Table CreateTableWithColumns(params (string, Justify)[] columnPairs)
    {
        var table = new Table
        {
            Border = TableBorder.Rounded,
            Expand = true
        };

        foreach ((string, Justify) pair in columnPairs)
        {
            var column = new TableColumn(pair.Item1);

            switch (pair.Item2)
            {
                case Justify.Center:
                    column = column.Centered();
                    break;
                case Justify.Right:
                    column = column.RightAligned();
                    break;
            }

            table.AddColumn(column);
        }

        return table;
    }

    private static Table AppendDataRowsToTable(Table table,
                                               ImmutableArray<MediaFile> orderedMediaFiles,
                                               IPrinter printer)
    {
        foreach (MediaFile mediaFile in orderedMediaFiles)
        {
            try
            {
                table.AddRow(MediaFileViewer.PrintFileSummary(mediaFile));
            }
            catch (TagLib.CorruptFileException e)
            {
                printer.Error("The file's tag metadata was corrupt or missing: " + e.Message);
            }
            catch (Exception e)
            {
                printer.Error($"An unknown error occurred with file \"{mediaFile.FileNameOnly}\": " + e.Message);
            }
        }

        return table;
    }
}
