using System.IO;
using Spectre.Console;

namespace AudioTagger.Console
{
    public sealed class MediaFileRenamer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                          DirectoryInfo workingDirectory,
                          IRegexCollection regexCollection,
                          IPrinter printer)
        {
            var directoryResponse = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"All files will be saved under directory \"{workingDirectory.FullName}\"")
                    .AddChoices(new[] {"Continue", "Cancel"}));

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

            var deletedDirectories = DeleteEmptySubDirectories(workingDirectory.FullName);
            if (deletedDirectories.Any())
            {
                printer.Print("DELETED DIRECTORIES:");
                foreach (var dir in deletedDirectories)
                    printer.Print("- " + dir);
            }
            else
            {
                printer.Print("No empty subdirectories found.");
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
            var newFolderName = HasValue(file.AlbumArtists)
                ? GetSafeString(string.Join(" && ", file.AlbumArtists))
                : HasValue(file.Artists)
                    ? GetSafeString(string.Join(" && ", file.Artists))
                    : "_UNSPECIFIED";
            var folderPath = Path.Combine(workingPath, newFolderName);

            var albumArtistText = HasValue(file.AlbumArtists)
                ? GetSafeString(string.Join(" && ", file.AlbumArtists)) + " ≡ "
                : string.Empty;
            var artistText = HasValue(file.Artists)
                ? GetSafeString(string.Join(" && ", file.Artists)) + " - "
                : string.Empty;
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
            // var genreText = file.Genres.Any()
            //     ? GetSafeString(" {" + string.Join("; ", file.Genres) + "}")
            //     : string.Empty;
            var ext = Path.GetExtension(file.FileNameOnly);

            var newFileName =
                albumArtistText +
                artistText +
                string.Concat(string.IsNullOrWhiteSpace(albumText)
                    ? new[] {titleText, yearText}
                    : new[] {albumText, yearText, " - ", trackText, titleText}) + ext;

            //var previousFolderFileName = Path.Combine(Directory.GetParent(file.Path).Name, file.FileNameOnly);
            var previousFolderFileName = file.Path.Replace(workingPath + Path.DirectorySeparatorChar, "");
            var proposedFolderFileName = Path.Combine(newFolderName, newFileName);
            // printer.Print("> " + previousFolderFileName); // Debug use
            // printer.Print("> " + proposedFolderFileName); // Debug use
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
                        .AddChoices(new[] {no, yes, yesToAll, cancel}));

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

        private static bool HasValue(IEnumerable<string> tagValues)
        {
            if (tagValues?.Any() != true)
                return false;

            var asString = string.Join("", tagValues);

            if (string.IsNullOrWhiteSpace(asString))
                return false;

            if (asString.Contains("<unknown>"))
                return false;

            return true;
        }

        /// <summary>
        /// Delete all empty subdirectories beneath, and including, the given one.
        /// </summary>
        /// <remarks>Implementation from https://stackoverflow.com/a/2811654/11767771</remarks>
        /// <param name="topDirectoryPath"></param>
        private static List<string> DeleteEmptySubDirectories(string topDirectoryPath)
        {
            var deletedDirectories = new List<string>();

            foreach (var directory in Directory.GetDirectories(topDirectoryPath))
            {
                DeleteEmptySubDirectories(directory);

                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                    deletedDirectories.Add(directory);
                }
            }

            return deletedDirectories;
        }
    }
}
