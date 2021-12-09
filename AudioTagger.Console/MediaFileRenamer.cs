namespace AudioTagger.Console
{
    public class MediaFileRenamer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, IPrinter printer)
        {
            ArgumentNullException.ThrowIfNull(mediaFiles);
            ArgumentNullException.ThrowIfNull(printer);

            if (!mediaFiles.Any())
            {
                printer.Error("No files to rename found");
                return;
            }

            foreach (var file in mediaFiles)
            {
                var artistsText = string.Join(" & ", file.Artists) + " - ";
                var albumText = string.IsNullOrWhiteSpace(file.Album)
                    ? string.Empty
                    : file.Album + " - ";
                var titleText = file.Title;
                var yearText = file.Year < 1000 ? string.Empty : " [" + file.Year.ToString() + "]";
                var genreText = file.Genres.Any()
                    ? " {" + string.Join("; ", file.Genres) + "}"
                    : string.Empty;

                var desiredFileName = string.Concat(
                    new string[] {
                        artistsText, albumText, titleText, yearText, genreText});

                // TODO: Print to the display
                // TODO: Rename the file
            }
        }
    }
}