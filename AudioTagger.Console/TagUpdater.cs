using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
                catch (Exception e)
                {
                    printer.Error("An error occurred in updating: " + e.Message);
                    printer.Print(e.StackTrace ?? "Stack trace not found.");
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

            var match = RegexCollection.GetFirstMatch(mediaFile);

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

            var proposedUpdates = updateableFields.GetUpdateOutput(mediaFile);

            if (proposedUpdates?.Any() != true)
            {
                printer.Print($"No updates needed for \"{mediaFile.FileNameOnly}\".",
                              ResultType.Neutral);
                return shouldCancel;
            }

            // Print the current tag data.
            printer.Print(OutputLine.GetTagPrintedLines(mediaFile), 1, 0);

            // Show the proposed updates and ask the user to confirm.
            printer.Print("Apply these updates?", 0, 0, "", ConsoleColor.Yellow);
            foreach (var update in proposedUpdates)
                printer.Print(update.Line);

            var response = ResponseHandler.AskUserYesNoCancel(printer);

            if (response == UserResponse.Cancel)
            {
                printer.Print("All operations cancelled.", ResultType.Cancelled, 1, 1);
                return true;
            }

            if (response == UserResponse.No)
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

            printer.Print("Updates saved", ResultType.Success, 0, 1);
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