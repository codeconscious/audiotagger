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
                    Printer.Print($"No file was submitted...", 0, 0, ResultSymbols.Failure, ConsoleColor.DarkRed);
                    continue;
                }

                var match = Regex.Match(fileData.FileNameFull, _regex);

                if (match == null)
                {
                    Printer.Print($"No regex match was found for \"{fileData.FileNameShort}\".", 0, 0, ResultSymbols.Failure, ConsoleColor.DarkRed);
                    continue;
                }

                var foundTags = match.Groups
                                     .OfType<Group>()
                                     .Where(g => g.Success);

                if (foundTags == null || !foundTags.Any())
                {
                    Printer.Print($"Could not parse data for filename \"{fileData.FileNameShort}.\"", 0, 0, ResultSymbols.Failure, ConsoleColor.DarkRed);
                    continue;
                }

                var updateables = new UpdatableFields(foundTags);

                var proposedUpdates = updateables.GetUpdateOutput(fileData);

                if (proposedUpdates == null || !proposedUpdates.Any())
                {
                    Printer.Print($"No updates for \"{fileData.FileNameShort}\".", 0, 0, ResultSymbols.Neutral, ConsoleColor.DarkGray);
                    continue;
                }

                Printer.Print(fileData.GetTagOutput(), 1, 0);

                Printer.Print("Apply the following proposed updates?", ConsoleColor.Yellow);

                foreach (var update in proposedUpdates)
                    Printer.Print(update.Line);

                Printer.Print(new List<LineSubString>
                {
                    new LineSubString("Press "),
                    new LineSubString("Y", ConsoleColor.Magenta),
                    new LineSubString(" or "),
                    new LineSubString("N", ConsoleColor.Magenta),
                    new LineSubString(" (or "),
                    new LineSubString("C", ConsoleColor.Magenta),
                    new LineSubString(" to cancel):  "),
                }, appendLines: 0);

                var validInput = false;
                var shouldUpdate = false;
                do
                {
                    var reply = Console.ReadKey();
                    if (reply.KeyChar == 'n' || reply.KeyChar == 'N' ||
                        reply.KeyChar == 'y' || reply.KeyChar == 'Y' ||
                        reply.KeyChar == 'c' || reply.KeyChar == 'C')
                    {
                        if (reply.KeyChar == 'c' || reply.KeyChar == 'C')
                        {
                            Console.WriteLine();
                            Printer.Print("All operations cancelled.", 1, 1, ResultSymbols.Cancelled, ConsoleColor.DarkRed);
                            return;
                        }

                        shouldUpdate = reply.KeyChar == 'y' || reply.KeyChar == 'Y';
                        validInput = true;
                    }
                }
                while (!validInput);

                if (!shouldUpdate)
                {
                    Printer.Print("No updates made", 1, 1, ResultSymbols.Failure);
                    continue;
                }

                if (updateables.Title != null && updateables.Title != fileData.Title)
                {
                    fileData.Title = updateables.Title;
                }                
                if (updateables.Artists?.All(a => fileData.Artists.Contains(a)) == false)
                {
                    fileData.Artists = updateables.Artists;
                }
                if (updateables.Year != null && updateables.Year != fileData.Year)
                {
                    fileData.Year = updateables.Year.Value;
                }
                if (updateables.Genres?.All(a => fileData.Genres.Contains(a)) == false)
                {
                    fileData.Genres = updateables.Genres;
                }

                try
                {
                    fileData.SaveUpdates();
                }
                catch (TagLib.CorruptFileException e)
                {
                    Printer.Error("The file's tag metadata was corrupt or missing.  " + e.Message);
                    continue;
                }                

                Printer.Print("Updates saved!", 1, 1, ResultSymbols.Success, ConsoleColor.DarkGreen);
            }
        }
    }
}