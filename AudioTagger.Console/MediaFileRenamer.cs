using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class MediaFileRenamer : IPathOperation
{
    private static readonly Regex TagFinderRegex = new(@"(?<=%)\w+(?=%)");

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
        RenameFiles(mediaFiles, workingDirectory, printer, settings.RenamePatterns);
        DeleteEmptySubDirectories(workingDirectory.FullName, printer);
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
                                    IEnumerable<string> renamePatterns)
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

                bool useRootPath = artistCounts[file.AlbumArtists.JoinWith(file.Artists)] == 1;

                isCancelRequested = RenameSingleFile(
                    file, printer, workingDirectory.FullName, useRootPath, ref doConfirm, renamePatterns);
            }
            catch (IOException ex)
            {
                printer.Error($"Error updating \"{file.FileNameOnly}\": {ex.Message}");
                printer.PrintException(ex);
                errors.Add(ex.Message); // The message should contain the file name.
            }
            catch (KeyNotFoundException ex)
            {
                printer.Error($"Error updating \"{file.FileNameOnly}\": {ex.Message}");
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
                                         bool keepInRootFolder,
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
            List<string> expectedTags = matches.Cast<Match>().Select(m => m.Value).ToList();
            if (expectedTags.Count == populatedTagNames.Count &&
                expectedTags.All(expectedTag => populatedTagNames.Contains(expectedTag)))
            {
                matchedRenamePattern = renamePattern;
            }
        }

        if (matchedRenamePattern is null)
        {
            printer.Error($"No appropriate rename pattern was found, so cannot rename \"{file.FileNameOnly}\"");
            return false;
        }

        string newFolderName = keepInRootFolder ? string.Empty : GenerateSafeDirectoryName(file);
        string fullFolderPath = Path.Combine(workingPath, newFolderName);
        string previousFolderFileName = file.Path.Replace(workingPath + Path.DirectorySeparatorChar, "");
        string newFileName = GenerateNewFileNameUsingTagData(file, populatedTagNames, matchedRenamePattern);
        string proposedFolderFileName = Path.Combine(workingPath, newFolderName, newFileName);

        if (previousFolderFileName == proposedFolderFileName)
        {
            printer.Print($"No rename needed for \"{file.Path.Replace(workingPath, "")}\"");
            return shouldCancel;
        }

        FileInfo currentFile = new(file.Path); // Create a duplicate file object for the new file.
        string newPathFileName = Path.Combine(workingPath, newFolderName, newFileName);
        string currentFullPath = file.Path.Replace(workingPath, "");
        string proposedFullPath = Path.Combine(newFolderName, newPathFileName).Replace(workingPath, "");

        if (currentFullPath == proposedFullPath)
        {
            printer.Print($"No rename needed for \"{currentFullPath}\".", fgColor: ConsoleColor.DarkGray);
            return shouldCancel;
        }

        printer.Print("   Current name: " + currentFullPath);
        printer.Print("  Proposed name: " + proposedFullPath, fgColor: ConsoleColor.Yellow);

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
                printer.Print("All operations cancelled.", ResultType.Cancelled, 1, 1);
                return true;
            }

            if (response == no)
            {
                printer.Print("Not renamed.", ResultType.Neutral, 0, 1);
                return shouldCancel;
            }

            if (response == yesToAll)
            {
                doConfirm = false; // Avoid asking again.
            }
        }

        if (!Directory.Exists(fullFolderPath))
            Directory.CreateDirectory(fullFolderPath);

        currentFile.MoveTo(newPathFileName);
        printer.Print("Rename OK");

        return shouldCancel;

        /// <summary>
        /// Generates and returns an updated filename using the given rename pattern and tag names.
        /// </summary>
        static string GenerateNewFileNameUsingTagData(
            MediaFile file,
            ICollection<string> fileTagNames,
            string renamePattern)
        {
            StringBuilder newBaseFileName =
                fileTagNames.Aggregate(
                    new StringBuilder(renamePattern),
                    (workingFileName, tagName) =>
                    {
                        return tagName switch
                        {
                            "ALBUMARTISTS" =>
                                workingFileName.Replace(
                                    "%ALBUMARTISTS%",
                                    IOUtilities.SanitizePath(file.AlbumArtists)),
                            "ARTISTS" =>
                                workingFileName.Replace(
                                    "%ARTISTS%",
                                    IOUtilities.SanitizePath(file.Artists)),
                            "ALBUM" =>
                                workingFileName.Replace(
                                    "%ALBUM%",
                                    IOUtilities.SanitizePath(file.Album)),
                            "TITLE" =>
                                workingFileName.Replace(
                                    "%TITLE%",
                                    IOUtilities.SanitizePath(file.Title)),
                            "YEAR" =>
                                workingFileName.Replace(
                                    "%YEAR%",
                                    IOUtilities.SanitizePath(file.Year.ToString())),
                            "TRACK" =>
                                workingFileName.Replace(
                                    "%TRACK%",
                                    IOUtilities.SanitizePath(file.TrackNo.ToString())),
                            _ => throw new InvalidOperationException(""),
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
    /// </summary>
    private static void DeleteEmptySubDirectories(string topDirectoryPath, IPrinter printer)
    {
        var deletedDirectories = new List<string>();

        foreach (string directory in Directory.GetDirectories(topDirectoryPath))
        {
            DeleteEmptySubDirectories(directory, printer);

            if (Directory.GetFiles(directory).Length == 0 &&
                Directory.GetDirectories(directory).Length == 0)
            {
                Directory.Delete(directory, false);
                deletedDirectories.Add(directory);
            }
        }

        PrintResults(deletedDirectories, printer);

        static void PrintResults(IList<string> deletedDirectories, IPrinter printer)
        {
            if (!deletedDirectories.Any())
            {
                return;
            }

            printer.Print("DELETED DIRECTORIES:");
            foreach (string dir in deletedDirectories)
                printer.Print("- " + dir);
        }
    }
}
