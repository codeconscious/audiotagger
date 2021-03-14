using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AudioTagger
{
    public static class Updater
    {
        // TODO: Perhaps get some a file, and allow several regexes
        private const string _regex = @"(?'Artists'.+) [-–] (?'Title'[^\[\{]+)(?: ?\[(?'Year'\d{3,})\])?(?: ?\{(?'Genres'.+)\})?";

        public static void UpdateTags(List<FileData?> filesData)
        {
            var regex = new Regex(_regex);

            foreach (var fileData in filesData)
            {
                if (fileData == null)
                {
                    Printer.Print($"No file was submitted.", 0, 0,
                                  ResultSymbols.Failure, ConsoleColor.DarkRed);
                    continue;
                }

                var match = Regex.Match(fileData.FileNameFull, _regex);

                if (match == null)
                {
                    Printer.Print($"Could not parse tags for \"{fileData.FileNameShort}\".",
                                  0, 0, ResultSymbols.Failure, ConsoleColor.DarkRed);
                    continue;
                }

                var foundTags = match.Groups
                                     .OfType<Group>()
                                     .Where(g => g.Success);

                if (foundTags == null || !foundTags.Any())
                {
                    Printer.Print($"Could not parse data for filename \"{fileData.FileNameShort}.\"",
                                  0, 0, ResultSymbols.Failure, ConsoleColor.DarkRed);
                    continue;
                }

                var updateableFields = new UpdatableFields(foundTags);

                var proposedUpdates = updateableFields.GetUpdateOutput(fileData);

                if (proposedUpdates == null || !proposedUpdates.Any())
                {
                    Printer.Print($"No updates needed for \"{fileData.FileNameShort}\".",
                                  0, 0, ResultSymbols.Neutral, ConsoleColor.DarkGray);
                    continue;
                }

                Printer.Print(fileData.GetTagOutput(), 1, 0);

                Printer.Print("Apply these updates?", ConsoleColor.Yellow);

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

                var validKeys = new List<char> { 'n', 'y', 'c' };
                var validInput = false;
                var doUpdate = false;
                do
                {
                    var keyInfo = Console.ReadKey();
                    var key = char.ToLowerInvariant(keyInfo.KeyChar);
                    if (validKeys.Contains(key))
                    {
                        if (key == 'c')
                        {
                            Console.WriteLine();
                            Printer.Print("All operations cancelled", 1, 1,
                                          ResultSymbols.Cancelled, ConsoleColor.DarkRed);
                            return;
                        }

                        doUpdate = key == 'y';
                        validInput = true;
                    }
                }
                while (!validInput);

                if (!doUpdate)
                {
                    Printer.Print("No updates made", 1, 1, ResultSymbols.Failure);
                    continue;
                }

                if (updateableFields.Title != null && updateableFields.Title != fileData.Title)
                {
                    fileData.Title = updateableFields.Title;
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

                try
                {
                    fileData.SaveUpdates();
                }
                catch (TagLib.CorruptFileException e)
                {
                    Printer.Error("The file's tag metadata was corrupt or missing. " + e.Message);
                    continue;
                }                

                Printer.Print("Updates saved", 1, 1, ResultSymbols.Success, ConsoleColor.DarkGreen);
            }
        }
    }
}