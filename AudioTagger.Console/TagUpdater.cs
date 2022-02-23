using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console
{
    public class TagUpdater : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, IPrinter printer)
        {
            bool isCancelled = false;

            // Process each file
            foreach (var mediaFile in mediaFiles)
            {
                try
                {
                    if (isCancelled)
                        break;

                    isCancelled = UpdateTags(mediaFile, printer);
                }
                catch (Exception ex)
                {
                    printer.Error($"Error updating {mediaFile.FileNameOnly}: {ex.Message}");
                    printer.PrintException(ex);
                    continue;
                }
            }
        }

        /// <summary>
        /// Make proposed tag updates to the specified file if the user agrees.
        /// </summary>
        /// <returns>A bool indicating whether the following file should be processed.</returns>
        private static bool UpdateTags(MediaFile mediaFile, IPrinter printer)
        {
            // TODO: Refactor cancellation so this isn't needed.
            const bool shouldCancel = false;

            var match = RegexCollection.GetFirstMatch(mediaFile.FileNameOnly);

            // If there are no regex matches against the filename, we cannot continue.
            if (match == null)
            {
                printer.Print($"Could not parse tags for \"{mediaFile.FileNameOnly}\".",
                              ResultType.Failure);
                return shouldCancel;
            }

            var matchedTags = match.Groups
                                   .OfType<Group>()
                                   .Where(g => g.Success);

            if (matchedTags?.Any() != true)
            {
                printer.Print($"Could not parse data for filename \"{mediaFile.FileNameOnly}.\"",
                                ResultType.Failure);
                return shouldCancel;
            }

            var updateableFields = new UpdatableFields(matchedTags);

            var proposedUpdates = updateableFields.GetUpdateKeyValuePairs(mediaFile);

            if (proposedUpdates?.Any() != true)
            {
                printer.Print($"No updates needed for \"{mediaFile.FileNameOnly}\".",
                              ResultType.Neutral);
                return shouldCancel;
            }

            // Print the filename
            //printer.PrintDivider(mediaFile.FileNameOnly, ConsoleColor.Cyan);

            // Print the current tag data.
            // printer.Print(OutputLine.GetTagPrintedLines(mediaFile), 1, 0);

            printer.PrintTagDataToTable(mediaFile, proposedUpdates);

            // Show the proposed updates and ask the user to confirm.
            // printer.Print("Apply these updates?", 0, 0, "", ConsoleColor.Yellow);
            // foreach (var update in proposedUpdates)
            //     printer.Print(update.Line);

            const string yes = "Yes";
            const string no = "No";
            const string cancel = "Cancel";

            var response = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Apply these updates?")
                    // .PageSize(10)
                    // .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
                    .AddChoices(new[] { no, yes, cancel }));


            // var response = ResponseHandler.AskUserYesNoCancel(printer);

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

            if (updateableFields.Genres?.All(a => fileData.Genres.Contains(a)) == false)
            {
                fileData.Genres = updateableFields.Genres;
            }
        }
    }
}