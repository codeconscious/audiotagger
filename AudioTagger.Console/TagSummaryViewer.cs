using Spectre.Console;

namespace AudioTagger.Console;

public class TagSummaryViewer : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                        DirectoryInfo workingDirectory,
                        IRegexCollection regexCollection,
                        IPrinter printer)
    {
        ArgumentNullException.ThrowIfNull(mediaFiles);

        printer.Print("Ordering the data...");
        var orderedMediaFiles = mediaFiles
            .OrderBy(m => string.Concat(m.AlbumArtists) ?? string.Empty)
            .ThenBy(m => string.Concat(m.Artists) ?? string.Empty)
            .ThenBy(m => m.Album ?? string.Empty)
            .ThenBy(m => m.TrackNo)
            .ThenBy(m => m.Title)
            .ToImmutableArray();

        var table = PrepareTableWithColumns("Artist(s)", "Album", "Track", "Title", "Year", "Genre(s)", "Length");

        var populatedTable = AppendDataRows(table,
                                            new MediaFileViewer(),
                                            orderedMediaFiles,
                                            printer);

        printer.Print("Printing the table...");
        AnsiConsole.Write(table);
    }

    private static Table PrepareTableWithColumns(params string[] columns)
    {
        var table = new Table
        {
            Border = TableBorder.Rounded,
            Expand = true
        };

        table.AddColumns(columns);

        return table;
    }

    private static Table AppendDataRows(Table table,
                                        MediaFileViewer viewer,
                                        ImmutableArray<MediaFile> orderedMediaFiles,
                                        IPrinter printer)
    {
        foreach (var mediaFile in orderedMediaFiles)
        {
            try
            {
                table.AddRow(viewer.PrintFileSummary(mediaFile));
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
