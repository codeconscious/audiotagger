using System.IO;
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

            for (var i = 0; i < mediaFiles.Count; i++)
            {
                var file = mediaFiles.ElementAt(i);

                var artistsText = string.Join(" & ", file.Artists) + " - ";
                var albumText = string.IsNullOrWhiteSpace(file.Album)
                    ? string.Empty
                    : file.Album + " - ";
                var titleText = file.Title;
                var yearText = file.Year < 1000 ? string.Empty : " [" + file.Year.ToString() + "]";
                var genreText = file.Genres.Any()
                    ? " {" + string.Join("; ", file.Genres) + "}"
                    : string.Empty;

                var newFileName = string.Concat(
                    new string[] {
                        artistsText, albumText, titleText, yearText, genreText});

                printer.Print("Desired name: " + newFileName);

                // TODO: Ask user to confirm the name change

                var currentFile = new FileInfo(file.Path);
                currentFile.MoveTo(currentFile.Directory.FullName + Path.DirectorySeparatorChar + newFileName + currentFile.Extension);
            }
        }
    }
}