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

            var orderedMediaFiles = mediaFiles
                .OrderBy(m => m.AlbumArtists.Length == 0 ? string.Empty : m.AlbumArtists[0])
                .ThenBy(m => m.Artists[0])
                .ThenBy(m => m.Album)
                .ThenBy(m => m.TrackNo)
                .ThenBy(m => m.Title);

            foreach (var mediaFile in orderedMediaFiles)
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
