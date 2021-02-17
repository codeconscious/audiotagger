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

            Print.FileData(fileData);

            var updateables = new UpdateableFields(foundElements);

            Print.Message("Proposed updates:"); // TODO: Should not be shown when none.

            byte proposedUpdateCount = 0;
            if (updateables.Title != null && updateables.Title != fileData.Title)
            {
                Console.WriteLine("  - Title: " + updateables.Title);
                proposedUpdateCount++;
            }
            if (updateables.Artists != null && !updateables.Artists.All(a => fileData.Artists.Contains(a)))
            {
                Console.WriteLine("  - Artists: " + string.Join("; ", updateables.Artists));
                proposedUpdateCount++;
            }
            if (updateables.Year != null && updateables.Year != fileData.Year)
            {
                Console.WriteLine("  - Year: " + updateables.Year);
                proposedUpdateCount++;
            }
            if (updateables.Genres != null && !updateables.Genres.All(a => fileData.Genres.Contains(a)))
            {
                Console.WriteLine("  - Genres: " + string.Join("; ", updateables.Genres));
                proposedUpdateCount++;
            }

            if (proposedUpdateCount == 0)
            {
                return (false, "No updates to make.", false);
            }

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