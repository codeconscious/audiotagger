using System.IO;
using Spectre.Console;

namespace AudioTagger.Console
{
    public sealed class MediaFileRenamer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, IPrinter printer)
        {
            var workingPath = Directory.GetParent(mediaFiles.First().Path).FullName;
            
            var directoryResponse = AnsiConsole.Prompt(
                 new SelectionPrompt<string>()
                     .Title($"All files will be saved under directory \"{workingPath}\"")
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

                    isCancelled = RenameFile(file, printer, workingPath, ref doConfirm);
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
            var folderName = file.Artists.Any()
                ? string.Join(" && ", file.Artists)
                : "_UNSPECIFIED";
            var folderPath = Path.Combine(workingPath, folderName);
            
            var albumText = string.IsNullOrWhiteSpace(file.Album)
                ? string.Empty
                : file.Album;
            var titleText = file.Title;
            var yearText = file.Year < 1000
                ? string.Empty
                : " [" + file.Year + "]";
            var genreText = file.Genres.Any()
                ? " {" + string.Join("; ", file.Genres) + "}"
                : string.Empty;

            var newFileName = string.Concat(string.IsNullOrWhiteSpace(albumText)
                ? new [] { titleText, yearText, genreText}
                : new [] { albumText, yearText, " - ", titleText, genreText});

            if (newFileName == file.FileNameOnly)
            {
                printer.Print($"No change needed for \"{file.FileNameOnly}\"");
                return shouldCancel;
            }
            
            // Create a duplicate file object for the new file.
            var currentFile = new FileInfo(file.Path);
            
            var newPathFileName = Path.Combine(workingPath, folderName, newFileName + currentFile.Extension);
            // printer.Print("NewPathFileName: " + newPathFileName);

            printer.Print("Current name: " + file.FileNameOnly);
            // printer.Print("Desired name: " + Path.GetDirectoryName(newPath) + Path.DirectorySeparatorChar + newFileName + currentFile.Extension);
            printer.Print("Updated name: " + Path.Combine(folderName, newPathFileName));

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

        private static string GetSafeCombinedPath(params string[] parts)
        {
            // foreach (var part in parts)
            // {
            //     foreach (var c in Path.GetInvalidFileNameChars()) 
            //     { 
            //         part = part.Replace(c, '-'); 
            //     }      
            // }

            var cleanedParts = parts.SelectMany(p =>
                Path.GetInvalidFileNameChars()
                    .Select(ch => p.Replace(ch, '-')))
                .ToArray();

            return Path.Combine(cleanedParts);
        }
    }
}