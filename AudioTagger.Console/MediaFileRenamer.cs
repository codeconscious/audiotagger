using System.IO;
using Spectre.Console;

namespace AudioTagger.Console
{
    public class MediaFileRenamer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, IPrinter printer)
        {
            bool isCancelled = false;
            var doConfirm = true;

            // Process each file
            for (var i = 0; i < mediaFiles.Count; i++)
            {
                var file = mediaFiles.ElementAt(i);

                try
                {
                    if (isCancelled)
                        break;

                    isCancelled = RenameFile(file, printer, ref doConfirm);
                }
                catch (Exception e)
                {
                    printer.Error($"Error updating {file.FileNameOnly}: {e.Message}");
                    printer.Print(e.StackTrace ?? "Stack trace not found.");
                    continue;
                }
            }
        }

        private bool RenameFile(MediaFile file, IPrinter printer, ref bool doConfirm)
        {
            ArgumentNullException.ThrowIfNull(file);
            ArgumentNullException.ThrowIfNull(printer);

            // TODO: Refactor cancellation so this isn't needed.
            const bool shouldCancel = false;

            // TODO: Move the loop to the outer method, as in TagUpdater?
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

            var currentFile = new FileInfo(file.Path);

            if (newFileName == file.FileNameOnly)
            {
                printer.Print($"No change needed for \"{file.FileNameOnly}\"");
                return shouldCancel;
            }

            printer.Print("Current name: " + file.FileNameOnly);
            printer.Print("Desired name: " + newFileName + currentFile.Extension);

            if (doConfirm)
            {
                const string no = "No";
                const string yes = "Yes";
                const string yesToAll = "Yes To All";
                const string cancel = "Cancel";

                var response = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Apply this update?")
                        .AddChoices(new[] { no, yes, yesToAll, cancel }));

                if (response == cancel)
                {
                    printer.Print("All operations cancelled.", ResultType.Cancelled, 1, 1);
                    return true;
                }

                if (response == no)
                {
                    printer.Print("No updates made", ResultType.Neutral, 0, 1);
                    return shouldCancel;
                }

                if (response == yesToAll)
                {
                    // Avoid asking next time.
                    doConfirm = false;
                }
            }

            currentFile.MoveTo(currentFile.Directory.FullName + Path.DirectorySeparatorChar + newFileName + currentFile.Extension);
            printer.Print("File renamed.");

            return shouldCancel;
        }
    }
}