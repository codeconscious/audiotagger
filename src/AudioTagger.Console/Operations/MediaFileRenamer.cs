using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;
using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

public sealed class MediaFileRenamer : IPathOperation
{
    private static readonly Regex TagFinderRegex = new(@"(?<=%)\w+(?=%)");

    private static readonly List<string> SafeToDeleteFileExtensions = [".DS_Store"];

    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        if (settings.Renaming is null)
        {
            printer.Error("The settings file contained no rename settings, so cannot continue.");
            return;
        }

        if (settings.Renaming.Patterns is null)
        {
            printer.Error("The rename settings contained no rename patterns, so cannot continue.");
            return;
        }

        printer.Print($"Found {settings.Renaming.Patterns.Count} rename patterns.");

        var eligibleMediaFiles = settings.Renaming.IgnoredDirectories is null
            ? mediaFiles
            : mediaFiles
                .Where(file => !settings.Renaming.IgnoredDirectories.Contains(file.ParentDirectoryName))
                .ToList()
                .AsReadOnly();

        if (eligibleMediaFiles.Count == 0)
        {
            printer.Warning($"None of the {mediaFiles.Count} files provided are eligible for renaming.");
            return;
        }

        if (mediaFiles.Count == eligibleMediaFiles.Count)
        {
            printer.Print("All files are eligible for renaming.");
        }
        else
        {
            var diff = mediaFiles.Count - eligibleMediaFiles.Count;
            var isAre = diff == 1 ? "is" : "are";
            printer.Print($"Out of {mediaFiles.Count} files, {diff} {isAre} ineligible for renaming.");
        }

        if (!ConfirmStart(workingDirectory, printer))
        {
            return;
        }

        RenameFiles(
            eligibleMediaFiles,
            workingDirectory,
            printer,
            settings.Renaming.Patterns,
            settings.Renaming.UseAlbumDirectories);

        var deletedDirs = DeleteEmptySubDirectories(workingDirectory.FullName, printer);
        PrintDeletedDirectories(deletedDirs, printer);
    }

    /// <summary>
    /// Asks user to confirm whether they want to continue (true) or cancel (false).
    /// </summary>
    private static bool ConfirmStart(DirectoryInfo workingDirectory, IPrinter printer)
    {
        string directoryResponse = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                // Escaped because substrings like "[1984]" will be misinterpreted as formatting codes.
                .Title($"All files will be saved under directory \"{Markup.Escape(workingDirectory.FullName)}\"")
                .AddChoices("Continue", "Cancel"));

        if (directoryResponse == "Continue")
        {
            return true;
        }

        printer.Print("Cancelling...");
        return false;
    }

    private static void RenameFiles(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        IPrinter printer,
        IEnumerable<string> renamePatterns,
        bool useAlbumDirectories)
    {
        var isCancelRequested = false;
        var doConfirm = true;
        var errors = new List<string>();

        IDictionary<string, int> artistCounts = GetArtistCounts(mediaFiles);

        for (int i = 0; i < mediaFiles.Count; i++)
        {
            MediaFile file = mediaFiles.ElementAt(i);

            if (file.Title.Length == 0)
            {
                printer.Print($"Skipping \"{file.FileNameOnly}\" because it has no title.",
                              fgColor: ConsoleColor.DarkRed);
                continue;
            }

            if (isCancelRequested)
            {
                break;
            }

            try
            {
                // Ensure artists with multiple tracks are saved in subdirectories.
                bool useArtistDirectory = artistCounts[file.AlbumArtists.JoinWith(file.Artists)] > 1;

                isCancelRequested = RenameSingleFile(
                    file,
                    printer,
                    workingDirectory.FullName,
                    useArtistDirectory,
                    useAlbumDirectories,
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
            if (errors.None())
            {
                return;
            }

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
    private static bool RenameSingleFile(
        MediaFile file,
        IPrinter printer,
        string workingPath,
        bool useArtistDirectory,
        bool useAlbumDirectory,
        ref bool doConfirm,
        IEnumerable<string> renamePatterns)
    {
        ArgumentNullException.ThrowIfNull(file);

        const bool shouldCancel = false;

        var populatedTagNames = file.PopulatedTagNames();
        string? matchedRenamePattern = null;
        foreach (string pattern in renamePatterns)
        {
            var matches = TagFinderRegex.Matches(pattern);
            var expectedTags = matches.Select(m => m.Value).ToList();

            if (AreIdentical(expectedTags, populatedTagNames))
            {
                matchedRenamePattern = pattern;
            }
        }

        if (matchedRenamePattern is null)
        {
            printer.Warning($"No rename pattern found for \"{file.FileNameOnly}\".");
            return false;
        }

        MediaFilePathInfo oldPathInfo = new(workingPath, file.FileInfo.FullName);

        string newArtistDir = useArtistDirectory
            ? GenerateSafeDirectoryName(file)
            : string.Empty;
        string newAlbumDir = useAlbumDirectory && useArtistDirectory && file.Album.HasText()
            ? IoUtilities.SanitizePath(file.Album)
            : string.Empty;
        string newFileName = GenerateFileNameUsingPattern(file, populatedTagNames, matchedRenamePattern);
        MediaFilePathInfo newPathInfo = new(workingPath, [newArtistDir, newAlbumDir], newFileName);

        if (oldPathInfo.FullFilePath(true) == newPathInfo.FullFilePath(true))
        {
            printer.Print($"No rename needed for \"{oldPathInfo.FullFilePath(false)}\"", fgColor: ConsoleColor.DarkGray);
            return shouldCancel;
        }

        printer.Print("   Old name: " + oldPathInfo.FullFilePath(true));
        printer.Print("   New name: " + newPathInfo.FullFilePath(true));

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

        file.FileInfo.MoveTo(newPathInfo.FullFilePath(true), overwrite: false);
        printer.Print("Rename OK", fgColor: ConsoleColor.Green);

        return shouldCancel;

        /// <summary>
        /// Generates and returns a new filename by replacing placeholders within the rename
        /// pattern (e.g., `%ALBUM%`) with actual tag data from the `MediaFile`.
        /// </summary>
        static string GenerateFileNameUsingPattern(
            MediaFile file,
            ICollection<string> fileTagNames,
            string renamePattern)
        {
            StringBuilder workingFileName =
                fileTagNames.Aggregate(
                    new StringBuilder(renamePattern),
                    (workingName, tagName) => ReplacePlaceholders(workingName, tagName)
                );

            var ext = Path.GetExtension(file.FileNameOnly);
            var unsanitizedName = workingFileName + ext;
            return IoUtilities.SanitizePath(unsanitizedName);

            StringBuilder ReplacePlaceholders(StringBuilder workingName, string tagName)
            {
                return tagName switch
                    {
                        "ALBUMARTISTS" =>
                            workingName.Replace(
                                "%ALBUMARTISTS%",
                                IoUtilities.SanitizePath(file.AlbumArtists)),
                        "ARTISTS" =>
                            workingName.Replace(
                                "%ARTISTS%",
                                IoUtilities.SanitizePath(file.Artists)),
                        "ALBUM" =>
                            workingName.Replace(
                                "%ALBUM%",
                                IoUtilities.SanitizePath(file.Album)),
                        "TITLE" =>
                            workingName.Replace(
                                "%TITLE%",
                                IoUtilities.SanitizePath(file.Title)),
                        "YEAR" =>
                            workingName.Replace(
                                "%YEAR%",
                                IoUtilities.SanitizePath(file.Year.ToString())),
                        "TRACK" =>
                            workingName.Replace(
                                "%TRACK%",
                                IoUtilities.SanitizePath(file.TrackNo.ToString())),
                        _ => throw new InvalidOperationException($"File tag name \"{tagName} is not supported.")
                    };
            }
        }

        /// <summary>
        /// Generates and returns a directory name for a file given its tags. Never returns null.
        /// </summary>
        static string GenerateSafeDirectoryName(MediaFile file)
        {
            if (MediaFile.HasAnyValues(file.AlbumArtists))
            {
                return IoUtilities.SanitizePath(file.AlbumArtists);
            }

            if (MediaFile.HasAnyValues(file.Artists))
            {
                return IoUtilities.SanitizePath(file.Artists);
            }

            if (file.Album.HasText())
            {
                return IoUtilities.SanitizePath(file.Album);
            }

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

            // Skip directories containing subdirectories.
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
                    foreach (string file in dirFiles)
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

    private static void PrintDeletedDirectories(IList<string> dirs, IPrinter printer)
    {
        if (dirs.None())
        {
            printer.Print("No directories were deleted.");
            return;
        }

        printer.Print("Deleted directories:");
        foreach (string dir in dirs)
            printer.Print("- " + dir);
    }

    private static bool AreIdentical<T>(List<T> expectedTags, IList<T> populatedTagNames)
        => expectedTags.Count == populatedTagNames.Count &&
           expectedTags.TrueForAll(populatedTagNames.Contains);
}
