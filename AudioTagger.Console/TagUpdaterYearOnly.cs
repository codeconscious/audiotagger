using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public class TagUpdaterYearOnly : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IRegexCollection regexCollection,
                      IPrinter printer)
    {
        var isCancelled = false;
        var doConfirm = true;
        var errorFiles = new List<string>();

        // Process each file
        foreach (var mediaFile in mediaFiles)
        {
            try
            {
                isCancelled = UpdateTags(mediaFile, printer, ref doConfirm);

                if (isCancelled)
                    break;
            }
            catch (Exception ex)
            {
                printer.Error($"Error updating {mediaFile.FileNameOnly}: {ex.Message}");
                //printer.PrintException(ex);
                errorFiles.Add(mediaFile.FileNameOnly);
                continue;
            }
        }

        if (errorFiles.Any())
        {
            printer.Print("Files with errors:");
            errorFiles.ForEach(f => printer.Print("- " + f));
        }
    }

    /// <summary>
    /// Make proposed tag updates to the specified file if the user agrees.
    /// </summary>
    /// <returns>A bool indicating whether the following file should be processed.</returns>
    private static bool UpdateTags(MediaFile mediaFile, IPrinter printer, ref bool doConfirm)
    {
        // TODO: Refactor cancellation so this isn't needed.
        const bool shouldCancel = false;
        const string updateType = "year";

        var createdDt = System.IO.File.GetCreationTime(mediaFile.Path);

        var updateableFields = new UpdatableFields(updateType, createdDt.Year);

        var proposedUpdates = updateableFields.GetUpdateKeyValuePairs(mediaFile);

        if (proposedUpdates?.Any() != true)
        {
            printer.Print($"No {updateType} updates needed for \"{mediaFile.FileNameOnly}\".",
                          ResultType.Neutral);
            return shouldCancel;
        }

        printer.PrintTagDataToTable(mediaFile, proposedUpdates);

        if (doConfirm)
        {
            const string no = "No";
            const string yes = "Yes";
            const string yesToAll = "Yes To All";
            const string cancel = "Cancel";

            var response = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Apply these updates?")
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

        // Make the tag updates
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
    private static void UpdateFileTags(MediaFile fileData, UpdatableFields updateableFields)
    {
        if (updateableFields.Title != null && updateableFields.Title != fileData.Title)
        {
            fileData.Title = updateableFields.Title;
        }

        if (updateableFields.Album != null && updateableFields.Album != fileData.Album)
        {
            fileData.Album = updateableFields.Album;
        }

        if (updateableFields.Artists?.All(a => fileData.Artists.Contains(a)) == false)
        {
            fileData.Artists = updateableFields.Artists;
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
            fileData.Genres = updateableFields.Genres;
        }
    }
}
