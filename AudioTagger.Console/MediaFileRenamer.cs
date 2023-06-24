using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class MediaFileRenamer : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings? settings = null)
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
        var directoryResponse = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"All files will be saved under directory \"{workingDirectory.FullName}\"")
                .AddChoices(new[] {"Continue", "Cancel"}));

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

        var artistCounts = GetArtistCounts(mediaFiles);

        for (var i = 0; i < mediaFiles.Count; i++)
        {
            var file = mediaFiles.ElementAt(i);

            if (file.Title?.Length == 0)
            {
                printer.Print($"Skipping \"{file.FileNameOnly}\" because it has no title.", fgColor: ConsoleColor.DarkRed);
                continue;
            }

            try
            {
                if (isCancelRequested)
                    break;

                var useRootPath = artistCounts[GetConcatenatedArtists(file)] == 1;

                isCancelRequested = RenameSingleFile(
                    file, printer, workingDirectory.FullName, useRootPath, ref doConfirm, renamePatterns);
            }
            catch (IOException e)
            {
                printer.Error($"Error updating \"{file.FileNameOnly}\": {e.Message}");
                printer.PrintException(e);
                errors.Add(e.Message); // The message should contain the file name.
            }
            catch (KeyNotFoundException e)
            {
                printer.Error($"Error updating \"{file.FileNameOnly}\": {e.Message}");
                printer.PrintException(e);
                errors.Add(e.Message);
            }
        }

        PrintErrors(errors, printer);

        static void PrintErrors(IList<string> errors, IPrinter printer)
        {
            if (!errors.Any())
                return;

            uint number = 1;
            printer.Print("ERRORS:");
            foreach (var error in errors)
                printer.Print($" - #{number++}: {error}");
        }

        static IDictionary<string, int> GetArtistCounts(IReadOnlyCollection<MediaFile> mediaFiles)
        {
            return mediaFiles.GroupBy(n => GetConcatenatedArtists(n))
                .ToDictionary(g => g.Key,
                                g => g.Count());
        }
    }

    /// <summary>
    /// Renames a single file based upon its tags. The filename format is set manually.
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

        // printer.Print($"• FILENAME: {file.FileNameOnly} (with {renamePatterns.Count()} patterns)");

        ImmutableList<string> fileTagNames = file.PopulatedTagNames();

        string? matchedRenamePattern = null;
        var regex = new Regex(@"(?<=%)\w+(?=%)");
        foreach (var pattern in renamePatterns) // NOT REGEXES!
        {
            var matches = regex.Matches(pattern);
            var expectedTags = matches.Cast<Match>().Select(m => m.Value).ToImmutableList();

            if (expectedTags.Count == fileTagNames.Count &&
                expectedTags.All(expectedTag => fileTagNames.Contains(expectedTag!)))
            {
                // printer.Print("Match found!");
                matchedRenamePattern = pattern;
            }
        }

        if (matchedRenamePattern is null)
        {
            printer.Print($"No appropriate rename pattern was found, so cannot rename \"{file.FileNameOnly}\"",
                          fgColor: ConsoleColor.Red);
            return false;
        }
        // printer.Print($"Matched rename pattern: {matchedRenamePattern}");

        var newFileName = GenerateNewFileName(file, fileTagNames, matchedRenamePattern);
        // printer.Print($"New filename is: {newFileName}");

        var newFolderName = keepInRootFolder ? string.Empty : GenerateSafeDirectoryName(file);
        var fullFolderPath = Path.Combine(workingPath, newFolderName);
        var previousFolderFileName = file.Path.Replace(workingPath + Path.DirectorySeparatorChar, "");
        var proposedFolderFileName = Path.Combine(workingPath, newFolderName, newFileName);
        // printer.Print("> " + previousFolderFileName); // For debug use
        // printer.Print("> " + proposedFolderFileName); // For debug use

        if (previousFolderFileName == proposedFolderFileName)
        {
            printer.Print($"No rename needed for \"{file.Path.Replace(workingPath, "")}\"");
            return shouldCancel;
        }

        var currentFile = new FileInfo(file.Path); // Create a duplicate file object for the new file.

        var newPathFileName = Path.Combine(workingPath, newFolderName, newFileName);

        var currentFullPath = file.Path.Replace(workingPath, "");
        var proposedFullPath = Path.Combine(newFolderName, newPathFileName).Replace(workingPath, "");

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
        /// Replaces characters that are invalid in file path names with a safe character.
        /// </summary>
        /// <returns>A corrected string, or the original if no changes were needed.</returns>
        static string EnsurePathSafeString(string input)
        {
            var working = input;

            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                working = working.Replace(ch, '_');
            }

            return working;
        }

        static string GenerateNewFileName(MediaFile file, ICollection<string> fileTagNames, string renamePattern)
        {
            StringBuilder fileNameBuilder = new(renamePattern);
            foreach (var fileTagName in fileTagNames)
            {
                switch (fileTagName)
                {
                    case "ALBUMARTISTS":
                        fileNameBuilder.Replace("%ALBUMARTISTS%", EnsurePathSafeString(string.Join(" && ", file.AlbumArtists)));
                        break;
                    case "ARTISTS":
                        fileNameBuilder.Replace("%ARTISTS%", EnsurePathSafeString(string.Join(" && ", file.Artists)));
                        break;
                    case "ALBUM":
                        fileNameBuilder.Replace("%ALBUM%", EnsurePathSafeString(file.Album));
                        break;
                    case "TITLE":
                        fileNameBuilder.Replace("%TITLE%", EnsurePathSafeString(file.Title));
                        break;
                    case "YEAR":
                        fileNameBuilder.Replace("%YEAR%", EnsurePathSafeString(file.Year.ToString()));
                        break;
                    case "TRACK":
                        fileNameBuilder.Replace("%TRACK%", EnsurePathSafeString(file.TrackNo.ToString()));
                        break;
                }
            }
            return fileNameBuilder.ToString() + Path.GetExtension(file.FileNameOnly);
        }

        /// <summary>
        /// Generates and returns a directory name for a file given its tags. Never returns null.
        /// </summary>
        static string GenerateSafeDirectoryName(MediaFile file)
        {
            if (MediaFile.HasAnyValues(file.AlbumArtists))
                return EnsurePathSafeString(string.Join(" && ", file.AlbumArtists));

            if (MediaFile.HasAnyValues(file.Artists))
                return EnsurePathSafeString(string.Join(" && ", file.Artists));

            if (!string.IsNullOrWhiteSpace(file.Album))
                return EnsurePathSafeString(file.Album);

            return "___UNSPECIFIED___";
        }
    }

    /// <summary>
    /// Recursively deletes all empty subdirectories beneath, and including, the given one.
    /// </summary>
    private static void DeleteEmptySubDirectories(string topDirectoryPath, IPrinter printer)
    {
        var deletedDirectories = new List<string>();

        foreach (var directory in Directory.GetDirectories(topDirectoryPath))
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
            foreach (var dir in deletedDirectories)
                printer.Print("- " + dir);
        }
    }

    /// <summary>
    /// Reads the album artists, if any, or else the artists of a file
    /// and returns them in an unformatted, concatenated string.
    /// </summary>
    private static string GetConcatenatedArtists(MediaFile file)
    {
        // TODO: What if both fields are empty?
        return string.Concat(
            file.AlbumArtists.Any()
                ? file.AlbumArtists
                : file.Artists);
    }
}
