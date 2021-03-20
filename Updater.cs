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
            
            foreach (var fileData in filesData)
            {
                try
                {
                    if (isCancelled)
                        break;

                    isCancelled = UpdateTags(fileData);
                }
                catch (Exception e)
                {
                    Printer.Error("An error occurred in updating: " + e.Message);
                    continue;
                }
            }            
        }

        // TODO: Needs to be shorter.
        /// <summary>
        /// Update the tags of a specified file, if necessary.
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns>A bool indicating whether or not the following file should be processed.</returns>
        private static bool UpdateTags(FileData fileData)
        {
            var regexes = new RegexCollection("FileNameRegexes.txt");
            var shouldCancel = false;

            // This check needs to be handled earlier and better.
            if (fileData == null)
            {
                Printer.Print($"No file was submitted.", ResultType.Failure);
                return shouldCancel;
            }

            var match = regexes.GetFirstMatch(fileData);

            // If there are no regex matches against the filename, we cannot continue.
            if (match == null)
            {
                Printer.Print($"Could not parse tags for \"{fileData.FileNameOnly}\".",
                              ResultType.Failure);
                return shouldCancel;
            }

            var foundTags = match.Groups
                                 .OfType<Group>()
                                 .Where(g => g.Success);

            if (foundTags == null || !foundTags.Any())
            {
                Printer.Print($"Could not parse data for filename \"{fileData.FileNameOnly}.\"",
                                ResultType.Failure);
                return shouldCancel;
            }

            var updateableFields = new UpdatableFields(foundTags);

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

            Printer.Print(new LineSubString[]
            {
                new ("Press "),
                new ("Y", ConsoleColor.Magenta),
                new (" or "),
                new ("N", ConsoleColor.Magenta),
                new (" (or "),
                new ("C", ConsoleColor.Magenta),
                new (" to cancel):  "),
            }, appendLines: 0);

            var validKeys = new List<char> { 'n', 'y', 'c' }.AsReadOnly();
            var validInput = false;
            var doUpdate = false;
            do
            {
                var keyInfo = Console.ReadKey(); // TODO: Hide invalid entries
                var keyChar = char.ToLowerInvariant(keyInfo.KeyChar);
                if (validKeys.Contains(keyChar))
                {
                    if (keyChar == 'c')
                    {
                        Console.WriteLine();
                        Printer.Print("All operations cancelled",
                                        ResultType.Cancelled, 1, 1);
                        shouldCancel = true;
                        return shouldCancel;
                    }

                    doUpdate = keyChar == 'y';
                    validInput = true;
                }
            }
            while (!validInput);

            if (!doUpdate)
            {
                Printer.Print("No updates made", ResultType.Neutral, 1, 1);
                return shouldCancel;
            }

            // Make the necessary updates
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

            Printer.Print("Updates saved", ResultType.Success, 1, 1);
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