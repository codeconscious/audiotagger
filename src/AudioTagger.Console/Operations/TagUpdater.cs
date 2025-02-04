using System.Text.RegularExpressions;
using AudioTagger.Library;
using AudioTagger.Library.Genres;
using Spectre.Console;

namespace AudioTagger.Console.Operations;

public sealed class TagUpdater : IPathOperation
{
    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        bool doConfirm = true;
        List<string> errorFiles = [];

        var regexes = settings.Tagging?.RegexPatterns;
        if (regexes?.Any() != true)
        {
            throw new InvalidOperationException("No tagging regexes found in settings! Cannot continue.");
        }

        RegexCollection regexCollection = new(regexes);
        printer.Print($"Found {regexCollection.Patterns.Count} regex expression(s).");

        foreach (MediaFile mediaFile in mediaFiles)
        {
            try
            {
                var cancelRequested = UpdateTags(
                    mediaFile,
                    regexCollection,
                    printer,
                    settings,
                    ref doConfirm);

                if (cancelRequested)
                    break;
            }
            catch (Exception ex)
            {
                printer.Error($"Update error: {ex.Message}");
                errorFiles.Add(mediaFile.FileNameOnly);
            }
        }

        if (errorFiles.Count != 0)
        {
            printer.Print("Files with errors:");
            errorFiles.ForEach(f => printer.Print("- " + f));
        }
    }

    /// <summary>
    /// Make proposed tag updates to the specified file if the user agrees.
    /// </summary>
    /// <returns>A bool indicating whether the following file should be processed.</returns>
    private static bool UpdateTags(
        MediaFile mediaFile,
        RegexCollection regexCollection,
        IPrinter printer,
        Settings settings,
        ref bool doConfirm)
    {
        // TODO: Refactor cancellation so this isn't needed.
        const bool shouldCancel = false;

        Match? match = regexCollection.GetFirstMatch(mediaFile.FileNameOnly);

        // If there are no regex matches against the filename, we cannot continue.
        if (match == null)
        {
            printer.Print($"Could not parse tags for \"{mediaFile.FileNameOnly}\".",
                          ResultType.Failure);
            return shouldCancel;
        }

        IEnumerable<Group>? matchedTags = match.Groups
                               .OfType<Group>()
                               .Where(g => g.Success);

        if (matchedTags.Any() != true)
        {
            printer.Print($"Could not parse data for filename \"{mediaFile.FileNameOnly}.\"",
                            ResultType.Failure);
            return shouldCancel;
        }

        var artistsWithGenres =
            GenreService.Read(settings.ArtistGenreCsvFilePath) is { IsSuccess: true } result
                ? result.Value
                : [];

        UpdatableFields updateableFields = new(matchedTags, artistsWithGenres);
        Dictionary<string, string> proposedUpdates = updateableFields.GetUpdateKeyValuePairs(mediaFile);

        if (proposedUpdates.None())
        {
            printer.Print($"No updates needed for \"{mediaFile.FileNameOnly}\".",
                          ResultType.Neutral);
            return shouldCancel;
        }

        // Print the filename
        //printer.PrintDivider(mediaFile.FileNameOnly, ConsoleColor.Cyan);

        // Print the current tag data.
        // printer.Print(OutputLine.GetTagPrintedLines(mediaFile), 1, 0);

        printer.PrintTagDataToTable(mediaFile, proposedUpdates, false);

        // Show the proposed updates and ask the user to confirm.
        // printer.Print("Apply these updates?", 0, 0, string.Empty, ConsoleColor.Yellow);
        // foreach (var update in proposedUpdates)
        //     printer.Print(update.Line);

        if (doConfirm)
        {
            const string no = "No";
            const string yes = "Yes";
            const string yesToAll = "Yes To All";
            const string cancel = "Cancel";

            string response = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Apply these updates?")
                    .AddChoices(no, yes, yesToAll, cancel));

            if (response == cancel)
            {
                printer.Print("All operations cancelled.", ResultType.Cancelled, 1, 1);
                return true;
            }

            if (response == no)
            {
                printer.Print("No updates made.", ResultType.Neutral, 0, 1);
                return shouldCancel;
            }

            if (response == yesToAll)
            {
                // Avoid asking next time.
                doConfirm = false;
            }
        }

        UpdateFileTags(mediaFile, updateableFields);
        try
        {
            mediaFile.SaveUpdates();
        }
        catch (TagLib.CorruptFileException ex)
        {
            printer.Error("Saving failed: " + ex.Message);
            return shouldCancel;
        }

        //printer.Print("Updates saved", ResultType.Success, 0, 1);
        AnsiConsole.MarkupLine("[green]Updates saved[/]" + Environment.NewLine);
        return shouldCancel;
    }

    /// <summary>
    /// Update file tags where they differ from parsed filename data.
    /// </summary>
    /// <param name="fileData"></param>
    /// <param name="updateableFields"></param>
    private static void UpdateFileTags(MediaFile fileData,
                                       UpdatableFields updateableFields)
    {
        if (updateableFields.Title != null && updateableFields.Title != fileData.Title)
        {
            fileData.Title = updateableFields.Title;
        }

        if (updateableFields.Album != null && updateableFields.Album != fileData.Album)
        {
            fileData.Album = updateableFields.Album;
        }

        if (updateableFields.AlbumArtists?.All(a => fileData.AlbumArtists.Contains(a)) == false)
        {
            fileData.AlbumArtists = updateableFields.AlbumArtists.Distinct().ToArray();
        }

        if (updateableFields.Artists?.All(a => fileData.Artists.Contains(a)) == false)
        {
            fileData.Artists = updateableFields.Artists.Distinct().ToArray();
        }

        if (updateableFields.Year != null && updateableFields.Year != fileData.Year)
        {
            fileData.Year = updateableFields.Year.Value;
        }

        if (updateableFields.TrackNo != null && updateableFields.TrackNo != fileData.TrackNo)
        {
            fileData.TrackNo = updateableFields.TrackNo.Value;
        }

        if (updateableFields.Genres?.All(a => fileData.Genres.Contains(a)) == false)
        {
            fileData.Genres = updateableFields.Genres.Distinct().ToArray();
        }
    }
}
