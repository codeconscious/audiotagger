using Spectre.Console;

namespace AudioTagger.Console
{
    public class TagSummaryViewer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, DirectoryInfo workingDirectory, IPrinter printer)
        {
            ArgumentNullException.ThrowIfNull(mediaFiles);

            var table = new Table();
            table.AddColumns("Artist(s)", "Album", "Track", "Title", "Year", "Length");
            table.Border = TableBorder.Rounded;
            table.Expand = true;

            var viewer = new MediaFileViewer();

            // printer.Print("Ordering the data...");
            var orderedMediaFiles = mediaFiles
                .OrderBy(m => string.Concat(m.AlbumArtists) ?? string.Empty)
                .ThenBy(m => string.Concat(m.Artists) ?? string.Empty)
                .ThenBy(m => m.Album ?? string.Empty)
                .ThenBy(m => m.TrackNo)
                .ThenBy(m => m.Title)
                .ToImmutableArray();

            // printer.Print("Printing the data...");
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
                    printer.Error($"An unknown error occurred with file {mediaFile.FileNameOnly}: " + e.Message);
                }
            }

            AnsiConsole.Write(table);
        }
    }
}
