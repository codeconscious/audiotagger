using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class MediaFileRenamer : IPathOperation
{
    private static readonly Regex TagFinderRegex = new(@"(?<=%)\w+(?=%)");
    private static readonly List<string> SafeToDeleteFileExtensions = [".DS_Store"];

    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        if (!ConfirmContinue(workingDirectory, printer))
        {
            return;
        }

        if (settings?.RenamePatterns is null)
            throw new InvalidOperationException("The settings contained no rename patterns. Cannot continue.");

        printer.Print($"Found {settings.RenamePatterns.Count} rename patterns.");
        RenameFiles(mediaFiles, workingDirectory, printer, settings.RenamePatterns, settings.RenameUseAlbumFolders);

        List<string> deletedDirs = DeleteEmptySubDirectories(workingDirectory.FullName, printer);
        PrintDeletedDirectoryResults(deletedDirs, printer);
    }

    /// <summary>
    /// Asks user to confirm whether they want to continue (true) or cancel (false).
    /// </summary>
    private static bool ConfirmContinue(DirectoryInfo workingDirectory, IPrinter printer)
    {
        string directoryResponse = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                // Escaped because substrings like "[1984]" will be misinterpreted as formatting codes.
                .Title($"All files will be saved under directory \"{Markup.Escape(workingDirectory.FullName)}\"")
                .AddChoices(["Continue", "Cancel"]));

        if (directoryResponse == "Continue")
            return true;

        printer.Print("Cancelling...");
        return false;
    }

    private static void RenameFiles(IReadOnlyCollection<MediaFile> mediaFiles,
                                    DirectoryInfo workingDirectory,
                                    IPrinter printer,
                                    IEnumerable<string> renamePatterns,
                                    bool useAlbumFolders)
    {
        var isCancelRequested = false;
        var doConfirm = true;
        var errors = new List<string>();

        IDictionary<string, int> artistCounts = GetArtistCounts(mediaFiles);

        for (int i = 0; i < mediaFiles.Count; i++)
        {
            MediaFile file = mediaFiles.ElementAt(i);

            if (file.Title?.Length == 0)
            {
                printer.Print($"Skipping \"{file.FileNameOnly}\" because it has no title.",
                              fgColor: ConsoleColor.DarkRed);
                continue;
            }

            try
            {
                if (isCancelRequested)
                    break;

                // Ensure artists with multiple tracks are saved in folders.
                bool useArtistFolder = artistCounts[file.AlbumArtists.JoinWith(file.Artists)] > 1;

                isCancelRequested = RenameSingleFile(
                    file,
                    printer,
                    workingDirectory.FullName,
                    useArtistFolder,
                    useAlbumFolders,
                    ref doConfirm,
                    renamePatterns);
            }
            catch (IOException ex)
            {
                printer.Error($"Error renaming \"{file.FileNameOnly}\": {ex.Message}");
                printer.PrintException(ex);
                errors.Add(ex.Message); // The message should contain the file name.
            }
            catch (KeyNotFoundException ex)
            {
                printer.Error($"Error renaming \"{file.FileNameOnly}\": {ex.Message}");
                printer.PrintException(ex);
                errors.Add(ex.Message);
            }
        }

        PrintErrors(errors, printer);

        static void PrintErrors(IList<string> errors, IPrinter printer)
        {
            if (!errors.Any())
                return;

            uint number = 1;
            printer.Print("ERRORS:");
            foreach (string error in errors)
                printer.Print($" - #{number++}: {error}");
        }

        static IDictionary<string, int> GetArtistCounts(IReadOnlyCollection<MediaFile> mediaFiles)
        {
            return mediaFiles
                .GroupBy(file => file.AlbumArtists.JoinWith(file.Artists))
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }

    /// <summary>
    /// Renames a single file based upon its tags and specified rename patterns.
    /// </summary>
    /// <returns>A bool indicated whether the user cancelled the operation or not.</returns>
    private static bool RenameSingleFile(MediaFile file,
                                         IPrinter printer,
                                         string workingPath,
                                         bool useArtistFolder,
                                         bool useAlbumFolders,
                                         ref bool doConfirm,
                                         IEnumerable<string> renamePatterns)
    {
        ArgumentNullException.ThrowIfNull(file);

        // TODO: Refactor cancellation so this isn't needed.
        const bool shouldCancel = false;

        ImmutableList<string> populatedTagNames = file.PopulatedTagNames();
        string? matchedRenamePattern = null;
        foreach (string renamePattern in renamePatterns)
        {
            MatchCollection matches = TagFinderRegex.Matches(renamePattern);
            var expectedTags = matches.Cast<Match>().Select(m => m.Value).ToImmutableList();
            if (expectedTags.Count == populatedTagNames.Count &&
                expectedTags.All(tag => populatedTagNames.Contains(tag)))
            {
                matchedRenamePattern = renamePattern;
            }
        }

        if (matchedRenamePattern is null)
        {
            printer.Error($"No appropriate rename pattern was found, so cannot rename \"{file.FileNameOnly}\"");
            return false;
        }

        MediaFilePathInfo oldPathInfo = new(workingPath, file.Path);

        string newArtistDir = useArtistFolder
            ? GenerateSafeDirectoryName(file)
            : string.Empty;
        string newAlbumDir = useAlbumFolders && useArtistFolder && !string.IsNullOrWhiteSpace(file.Album)
            ? IOUtilities.SanitizePath(file.Album)
            : string.Empty;
        string newFileName = GenerateFileNameViaTagData(file, populatedTagNames, matchedRenamePattern);
        MediaFilePathInfo newPathInfo = new(workingPath, [newArtistDir, newAlbumDir], newFileName);

        if (oldPathInfo.FullFilePath(true) == newPathInfo.FullFilePath(true))
        {
            printer.Print($"No rename needed for \"{oldPathInfo.FullFilePath(false)}\"");
            return shouldCancel;
        }

        printer.Print("   Old name: " + oldPathInfo.FullFilePath(true));
        printer.Print("   New name: " + newPathInfo.FullFilePath(true));;

        if (doConfirm)
        {
            const string no = "No";
            const string yes = "Yes";
            const string yesToAll = "Yes To All";
            const string cancel = "Cancel";

            string response = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Rename this file?")
                    .AddChoices([no, yes, yesToAll, cancel]));

            if (response == cancel)
            {
                printer.Print("Renaming cancelled.", ResultType.Cancelled, 1, 1);
                return true;
            }

            if (response == no)
            {
                printer.Print("Not renamed.", ResultType.Neutral, 0, 1);
                return shouldCancel;
            }

            if (response == yesToAll)
            {
                doConfirm = false; // To avoid asking again.
            }
        }

        if (!Directory.Exists(newPathInfo.DirectoryPath(true)))
        {
            Directory.CreateDirectory(newPathInfo.DirectoryPath(true));
        }

        FileInfo currentFile = new(file.Path); // Create a duplicate file object for the renamed file.
        currentFile.MoveTo(newPathInfo.FullFilePath(true), overwrite: false);
        printer.Print("Rename OK", fgColor: ConsoleColor.Green);

        return shouldCancel;

        /// <summary>
        /// Generates and returns an updated filename using the given rename pattern and tag names.
        /// </summary>
        static string GenerateFileNameViaTagData(
            MediaFile file,
            ICollection<string> fileTagNames,
            string renamePattern)
        {
            StringBuilder newBaseFileName =
                fileTagNames.Aggregate(
                    new StringBuilder(renamePattern),
                    (workingNameSb, tagName) =>
                    {
                        return tagName switch
                        {
                            "ALBUMARTISTS" =>
                                workingNameSb.Replace(
                                    "%ALBUMARTISTS%",
                                    IOUtilities.SanitizePath(file.AlbumArtists)),
                            "ARTISTS" =>
                                workingNameSb.Replace(
                                    "%ARTISTS%",
                                    IOUtilities.SanitizePath(file.Artists)),
                            "ALBUM" =>
                                workingNameSb.Replace(
                                    "%ALBUM%",
                                    IOUtilities.SanitizePath(file.Album)),
                            "TITLE" =>
                                workingNameSb.Replace(
                                    "%TITLE%",
                                    IOUtilities.SanitizePath(file.Title)),
                            "YEAR" =>
                                workingNameSb.Replace(
                                    "%YEAR%",
                                    IOUtilities.SanitizePath(file.Year.ToString())),
                            "TRACK" =>
                                workingNameSb.Replace(
                                    "%TRACK%",
                                    IOUtilities.SanitizePath(file.TrackNo.ToString())),
                            _ => throw new InvalidOperationException($"File tag name \"{tagName} is not supported."),
                        };
                    }
                );

            string unsanitizedName = newBaseFileName.ToString() + Path.GetExtension(file.FileNameOnly);
            return IOUtilities.SanitizePath(unsanitizedName);
        }

        /// <summary>
        /// Generates and returns a directory name for a file given its tags. Never returns null.
        /// </summary>
        static string GenerateSafeDirectoryName(MediaFile file)
        {
            if (MediaFile.HasAnyValues(file.AlbumArtists))
                return IOUtilities.SanitizePath(file.AlbumArtists);

            if (MediaFile.HasAnyValues(file.Artists))
                return IOUtilities.SanitizePath(file.Artists);

            if (!string.IsNullOrWhiteSpace(file.Album))
                return IOUtilities.SanitizePath(file.Album);

            return "___UNSPECIFIED___";
        }
    }

    /// <summary>
    /// Recursively deletes all empty subdirectories beneath, and including, the given one.
    /// Deletes standard system files, such as macOS `.DS_Store` files.
    /// </summary>
    private static List<string> DeleteEmptySubDirectories(string topDirectoryPath, IPrinter printer)
    {
        var deletedDirs = new List<string>();

        foreach (string dir in Directory.GetDirectories(topDirectoryPath))
        {
            deletedDirs.AddRange(DeleteEmptySubDirectories(dir, printer));
            string[] dirFiles = Directory.GetFiles(dir);

            // Skip directories containing subfolders.
            if (Directory.GetDirectories(dir).Length > 0)
            {
                continue;
            }

            if (dirFiles.Length == 0)
            {
                try
                {
                    Directory.Delete(dir, recursive: false);
                    deletedDirs.Add(dir);
                }
                catch (Exception ex)
                {
                    printer.Error(ex.Message);
                }
            }
            else if (dirFiles.All(file => SafeToDeleteFileExtensions.Any(file.EndsWith)))
            {
                try
                {
                    foreach (var file in dirFiles)
                    {
                        File.Delete(file);
                        Directory.Delete(dir, recursive: false);
                        deletedDirs.Add(dir);
                    }
                }
                catch (Exception ex)
                {
                    printer.Error(ex.Message);
                }

            }
        }

        return deletedDirs;
    }

    private static void PrintDeletedDirectoryResults(IList<string> dirs, IPrinter printer)
    {
        if (!dirs.Any())
        {
            printer.Print("No directories were deleted.");
            return;
        }

        printer.Print("Deleted directories:");
        foreach (string dir in dirs)
            printer.Print("- " + dir);
    }
}
