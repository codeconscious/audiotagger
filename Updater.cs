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

        public static (bool updatesDone, string message, bool cancel) UpdateTags(FileData fileData)
        {
            var regex = new Regex(_regex);

            var match = Regex.Match(fileData.FileName, _regex);

            if (match == null)
                return (false, "No match was found.", false);

            var foundTags = match.Groups
                                 .OfType<Group>()
                                 .Where(g => g.Success);

            if (foundTags == null || !foundTags.Any())
                return (false, "No tag groups successfully found. (Check the filename format.)", false);

            var updateables = new UpdateableFields(foundTags);

            var proposedUpdates = updateables.GetUpdateOutput(fileData);

            if (proposedUpdates == null || !proposedUpdates.Any())
            {
                return (false, "No updates: " + Path.GetFileName(fileData.FileName), false);
            }

            Printer.Print(fileData.GetTagsAsOutputLines());

            Printer.Print("Proposed updates:");
            foreach (var update in proposedUpdates)
                Printer.Print(update.Line);

            Console.WriteLine("Do you want to apply these updates to the file?");
            Console.Write("Enter Y or N (or C to cancel):  ");

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
                        return (false, "All operations cancelled!", true);
                    }

                    shouldUpdate = reply.KeyChar == 'y' || reply.KeyChar == 'Y';
                    validInput = true;
                }
            }
            while (!validInput);
            Console.WriteLine();

            if (!shouldUpdate)
                return (false, "No updates made", false);

            //byte updatesMade = 0;
            if (updateables.Title != null && updateables.Title != fileData.Title)
            {
                fileData.Title = updateables.Title;
                //updatesMade++;
            }                
            if (updateables.Artists?.All(a => fileData.Artists.Contains(a)) == false)
            {
                fileData.Artists = updateables.Artists;
                //updatesMade++;
            }
            if (updateables.Year != null && updateables.Year != fileData.Year)
            {
                fileData.Year = updateables.Year.Value;
                //updatesMade++;
            }
            if (updateables.Genres?.All(a => fileData.Genres.Contains(a)) == false)
            {
                fileData.Genres = updateables.Genres;
                //updatesMade++;
            }

            fileData.SaveUpdates();

            return (true, "Updates saved!", false);
        }
    }
}