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
        public void Start(IReadOnlyCollection<FileData> filesData)
        {
            bool isCancelled = false;

            // Get regexes
            const string regexFileName = "FileNameRegexes.txt";
            RegexCollection regexes;
            try
            {
                regexes = new RegexCollection(regexFileName);
            }
            catch (FileNotFoundException)
            {
                Printer.Print($"Regex file \"regexFileName\" not found!", ResultType.Failure);
                return;
            }
            catch (Exception ex)
            {
                Printer.Print($"Update error. Cannot continue. {ex.Message}", ResultType.Failure);
                return;
            }

            if (regexes == null || !regexes.Regexes.Any())
            {
                Printer.Print($"No regexes found. Cannot continue.", ResultType.Failure);
                return;
            }

            // Process each file
            foreach (var fileData in filesData)
            {
                try
                {
                    if (isCancelled)
                        break;

                    isCancelled = UpdateTags(fileData, regexes);
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
        /// <param name="regexes"></param>
        /// <returns>A bool indicating whether the following file should be processed.</returns>
        private static bool UpdateTags(FileData fileData, RegexCollection regexes)
        {
            var shouldCancel = false;

            var match = regexes.GetFirstMatch(fileData);

            // If there are no regex matches against the filename, we cannot continue.
            if (match == null)
            {
                Printer.Print($"Could not parse tags for \"{fileData.FileNameOnly}\".",
                              ResultType.Failure);
                return shouldCancel;
            }

            var matchedTags = match.Groups
                                 .OfType<Group>()
                                 .Where(g => g.Success);

            if (matchedTags == null || !matchedTags.Any())
            {
                Printer.Print($"Could not parse data for filename \"{fileData.FileNameOnly}.\"",
                                ResultType.Failure);
                return shouldCancel;
            }

            var updateableFields = new UpdatableFields(matchedTags);

            var proposedUpdates = updateableFields.GetUpdateOutput(fileData);

            if (proposedUpdates == null || !proposedUpdates.Any())
            {
                Printer.Print($"No updates needed for \"{fileData.FileNameOnly}\".",
                                ResultType.Neutral);
                return shouldCancel;
            }

            Printer.Print(fileData.GetTagPrintedLines(), 1, 0);

            Printer.Print("Apply these updates?", 0, 0, "", ConsoleColor.Yellow);

            foreach (var update in proposedUpdates)
                Printer.Print(update.Line);

            var question = new LineSubString[]
            {
                new ("Press "),
                new ("Y", ConsoleColor.Magenta),
                new (" or "),
                new ("N", ConsoleColor.Magenta),
                new (" (or "),
                new ("C", ConsoleColor.Magenta),
                new (" to cancel):  "),
            };

            var allowedResponses = new Dictionary<char, UserReponse>
            {
                { 'y', UserReponse.Yes },
                { 'n', UserReponse.No },
                { 'c', UserReponse.Cancel }
            };

            var response = ResponseHandler.GetUserResponse(question, allowedResponses);

            if (response == UserReponse.None)
            {
                Printer.Print("Error reading user input. Skipping this file...", ResultType.Failure);
                return shouldCancel;
            }

            if (response == UserReponse.Cancel)
            {
                Printer.Print("All operations cancelled.", ResultType.Cancelled, 1, 1);
                return true;
            }

            if (response == UserReponse.No)
            {
                Printer.Print("No updates made", ResultType.Neutral, 0, 1);
                return shouldCancel;
            }

            // Make the necessary tag updates
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