using System.IO;
using Spectre.Console;

namespace AudioTagger.Console
{
    public sealed class MediaFileRenamer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, DirectoryInfo workingDirectory, IPrinter printer)
        {
            var directoryResponse = AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                     .Title($"All files will be saved under directory \"{workingDirectory.FullName}\"")
                     .AddChoices(new[] { "Continue", "Cancel" }));

            if (directoryResponse == "Cancel")
            {
                printer.Print("Cancelling...");
                return;
            }
            
            var isCancelled = false;
            var doConfirm = true;

            // Process each file
            for (var i = 0; i < mediaFiles.Count; i++)
            {
                var file = mediaFiles.ElementAt(i);

                try
                {
                    if (isCancelled)
                        break;

                    isCancelled = RenameFile(file, printer, workingDirectory.FullName, ref doConfirm);
                }
                catch (Exception e)
                {
                    printer.Error($"Error updating \"{file.FileNameOnly}\": {e.Message}");
                    printer.PrintException(e);
                    continue;
                }
            }
        }

        private bool RenameFile(MediaFile file, IPrinter printer, string workingPath, ref bool doConfirm)
        {
            ArgumentNullException.ThrowIfNull(file);
            ArgumentNullException.ThrowIfNull(printer);

            // TODO: Refactor cancellation so this isn't needed.
            const bool shouldCancel = false;

            // TODO: Move the loop to the outer method, as in TagUpdater?
            //var albumArtistsText = string.Join(" & ", file.AlbumArtists) + " ≡ ";
            var newFolderName = file.Artists.Any()
                ? GetSafeString(string.Join(" && ", file.Artists))
                : "_UNSPECIFIED";
            var folderPath = Path.Combine(workingPath, newFolderName);
            
            var albumText = string.IsNullOrWhiteSpace(file.Album)
                ? string.Empty
                : GetSafeString(file.Album);
            var titleText = GetSafeString(file.Title);
            var yearText = file.Year < 1000
                ? string.Empty
                : " [" + file.Year + "]";
            var genreText = file.Genres.Any()
                ? GetSafeString(" {" + string.Join("; ", file.Genres) + "}")
                : string.Empty;

            var newFileName = string.Concat(string.IsNullOrWhiteSpace(albumText)
                ? new [] { titleText, yearText, genreText}
                : new [] { albumText, yearText, " - ", titleText, genreText});

            var previousFolderFileName = Path.Combine(newFolderName, newFileName + Path.GetExtension(file.FileNameOnly));
            var proposedFolderFileName = Path.Combine(Directory.GetParent(file.Path).Name, file.FileNameOnly);
            // printer.Print("> " + previousFolderFileName);
            // printer.Print("> " + proposedFolderFileName);
            if (previousFolderFileName == proposedFolderFileName)
            {
                printer.Print($"No change needed for \"{file.Path.Replace(workingPath, "")}\"");
                return shouldCancel;
            }
            
            // Create a duplicate file object for the new file.
            var currentFile = new FileInfo(file.Path);
            
            var newPathFileName = Path.Combine(workingPath, newFolderName, newFileName + currentFile.Extension);
            // printer.Print("NewPathFileName: " + newPathFileName);

            printer.Print(" Current name: " + file.FileNameOnly);
            // printer.Print("Desired name: " + Path.GetDirectoryName(newPath) + Path.DirectorySeparatorChar + newFileName + currentFile.Extension);
            printer.Print("Proposed name: " + Path.Combine(newFolderName, newPathFileName));

            if (doConfirm)
            {
                const string no = "No";
                const string yes = "Yes";
                const string yesToAll = "Yes To All";
                const string cancel = "Cancel";

                var response = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Rename this file?")
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
            
            // printer.Print(">> " + folderPath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // printer.Print("--> " + newPathFileName);
            currentFile.MoveTo(newPathFileName);
            printer.Print("Rename OK");
            
            // TODO: Delete any empty folders that remain.

            return shouldCancel;
        }

        private static string GetSafeString(string input)
        {
            var partWorking = input;
            
            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                partWorking = partWorking.Replace(ch, '_');
            }

            return partWorking;
        }
    }
}