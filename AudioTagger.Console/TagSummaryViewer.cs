using Spectre.Console;

namespace AudioTagger.Console
{
    public class TagSummaryViewer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, IPrinter printer)
        {
            ArgumentNullException.ThrowIfNull(mediaFiles);

            var table = new Table();
            table.AddColumns("Artist(s)", "Album", "Track", "Title", "Year", "Duration");
            table.Border = TableBorder.Rounded;
            table.Expand = true;

            var viewer = new MediaFileViewer();

            foreach (var mediaFile in mediaFiles//.OrderBy(m => m.Artists)
                                                // .ThenBy(m => m.Album)
                                                // .ThenBy(m => m.TrackNo)
                                                // .ThenBy(m => m.Title))
                                                )
            {
                try
                {
                    table.AddRow(viewer.PrintFileSummary(mediaFile));
                }
                catch (TagLib.CorruptFileException e)
                {
                    printer.Error("The file's tag metadata was corrupt or missing: " + e.Message);
                    continue;
                }
                catch (Exception e)
                {
                    printer.Error($"An unknown error occurred with file {mediaFile.FileNameOnly}: " + e.Message);
                    continue;
                }
            }

            AnsiConsole.Write(table);
        }
    }
}
