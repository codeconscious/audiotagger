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
            var errors = new List<string>();

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
                catch (IOException e)
                {
                    printer.Error($"Error updating \"{file.FileNameOnly}\": {e.Message}");
                    printer.PrintException(e);
                    errors.Add(e.Message); // The message should contain the file name.
                    continue;
                }
            }

            //Print the errors
            if (errors.Any())
            {
                printer.Print("ERRORS:");
                var number = 1;
                foreach (var error in errors)
                    printer.Print($" - #{number++}: {error}");
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
            var trackText = file.TrackNo == 0
                ? string.Empty
                : file.TrackNo.ToString("000") + " - ";
            var yearText = file.Year < 1000
                ? string.Empty
                : " [" + file.Year + "]";
            var genreText = file.Genres.Any()
                ? GetSafeString(" {" + string.Join("; ", file.Genres) + "}")
                : string.Empty;
            var ext = Path.GetExtension(file.FileNameOnly);

            var newFileName = string.Concat(string.IsNullOrWhiteSpace(albumText)
                ? new [] { titleText, yearText, genreText}
                : new [] { albumText, yearText, " - ", trackText, titleText, genreText}) + ext;

            //var previousFolderFileName = Path.Combine(Directory.GetParent(file.Path).Name, file.FileNameOnly);
            var previousFolderFileName = file.Path.Replace(workingPath + Path.DirectorySeparatorChar, "");
            var proposedFolderFileName = Path.Combine(newFolderName, newFileName);
            printer.Print("> " + previousFolderFileName);
            printer.Print("> " + proposedFolderFileName);
            if (previousFolderFileName == proposedFolderFileName)
            {
                printer.Print($"No change needed for \"{file.Path.Replace(workingPath, "")}\"");
                return shouldCancel;
            }

            // Create a duplicate file object for the new file.
            var currentFile = new FileInfo(file.Path);

            var newPathFileName = Path.Combine(workingPath, newFolderName, newFileName);
            // printer.Print("NewPathFileName: " + newPathFileName);

            // printer.Print("newFolderName: " + newFolderName);
            printer.Print(" Current name: " + file.Path.Replace(workingPath, ""));
            printer.Print("Proposed name: " + Path.Combine(newFolderName, newPathFileName).Replace(workingPath, ""));

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
            //workingPath

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

        // private static List<string> DeleteEmptySubDirectories(path)
        // {
        //
        //     // TODO: Return  a multi-value keyed collection of keyed to exception types instead?
        //     var errorDirectories = new List<string>();
        //     foreach (var path in paths)
        //     {
        //         try
        //         {
        //             Directory.Delete(path, true);
        //         }
        //         catch (Exception e)
        //         {
        //             errorDirectories.Add(path);
        //         }
        //     }
        //     return errorDirectories;
        // }

    }
}