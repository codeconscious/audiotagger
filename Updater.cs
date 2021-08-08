using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AudioTagger
{
    public class TagUpdater : IPathProcessor
    {
        public void Start(IReadOnlyCollection<FileData> filesData, IPrinter printer)
        {
            bool isCancelled = false;

           // Process each file
            foreach (var fileData in filesData)
            {
                try
                {
                    if (isCancelled)
                        break;

                    isCancelled = UpdateTags(fileData, printer);
                }
                catch (Exception e)
                {
                    Printer.Error("An error occurred in updating: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    continue;
                }
            }
        }

        /// <summary>
        /// Update the tags of a specified file, if necessary.
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns>A bool indicating whether the following file should be processed.</returns>
        private static bool UpdateTags(FileData fileData, IPrinter printer)
        {
            // TODO: Refactor so this isn't needed.
            const bool shouldCancel = false;

            var match = RegexCollection.GetFirstMatch(fileData);

            // If there are no regex matches against the filename, we cannot continue.
            if (match == null)
            {
                printer.Print($"Could not parse tags for \"{fileData.FileNameOnly}\".",
                              ResultType.Failure);
                return shouldCancel;
            }

            var matchedTags = match.Groups
                                   .OfType<Group>()
                                   .Where(g => g.Success);

            if (matchedTags?.Any() != true)
            {
                printer.Print($"Could not parse data for filename \"{fileData.FileNameOnly}.\"",
                                ResultType.Failure);
                return shouldCancel;
            }

            var updateableFields = new UpdatableFields(matchedTags);

            var proposedUpdates = updateableFields.GetUpdateOutput(fileData, printer);

            if (proposedUpdates?.Any() != true)
            {
                printer.Print($"No updates needed for \"{fileData.FileNameOnly}\".",
                              ResultType.Neutral);
                return shouldCancel;
            }

            Printer.GetTagPrintedLines(fileData); //, 1, 0);

            printer.Print("Apply these updates?", 0, 0, "", ConsoleColor.Yellow);

            foreach (var update in proposedUpdates)
                Printer.Print(update.Line);

            var response = ResponseHandler.AskUserYesNoCancel();

            if (response == UserResponse.Cancel)
            {
                Printer.Print("All operations cancelled.", ResultType.Cancelled, 1, 1);
                return true;
            }

            if (response == UserResponse.No)
            {
                Printer.Print("No updates made", ResultType.Neutral, 0, 1);
                return shouldCancel;
            }

            // Make the needed tag updates
            UpdateFileTags(fileData, updateableFields);

            try
            {
                fileData.SaveUpdates();
            }
            catch (TagLib.CorruptFileException ex)
            {
                Printer.Error("Saving failed: " + ex.Message);
                return shouldCancel;
            }

            Printer.Print("Updates saved", ResultType.Success, 0, 1);
            return shouldCancel;
        }

        /// <summary>
        /// Update file tags where they differ from filename data.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="updateableFields"></param>
        private static void UpdateFileTags(FileData fileData, UpdatableFields updateableFields)
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