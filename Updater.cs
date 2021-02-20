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

            var foundElements = match.Groups
                                     .OfType<Group>()
                                     .Where(g => g.Success);

            if (foundElements == null || !foundElements.Any())
                return (false, "No successful match groups found. (Check the filename format.)", false);

            var updateables = new UpdateableFields(foundElements);

            var proposedUpdates = new List<string>();
            if (updateables.Title != null && updateables.Title != fileData.Title)
                proposedUpdates.Add("  - Title: " + updateables.Title);
            if (updateables.Artists != null && !updateables.Artists.All(a => fileData.Artists.Contains(a)))
                proposedUpdates.Add("  - Artists: " + string.Join("; ", updateables.Artists));
            if (updateables.Year != null && updateables.Year != fileData.Year)
                proposedUpdates.Add("  - Year: " + updateables.Year);
            if (updateables.Genres != null && !updateables.Genres.All(a => fileData.Genres.Contains(a)))
                proposedUpdates.Add("  - Genres: " + string.Join("; ", updateables.Genres));

            if (!proposedUpdates.Any())
            {
                return (false, "No updates: " + Path.GetFileName(fileData.FileName), false);
            }

            Print.FileData(fileData, "  • ", 1);

            Print.Message("Proposed updates:"); // TODO: Should not be shown when none.
            foreach (var update in proposedUpdates)
                Console.WriteLine(update);

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