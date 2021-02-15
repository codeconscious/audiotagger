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

        public static (bool success, string message) UpdateTags(FileData fileData)
        {
            var regex = new Regex(_regex);

            var match = Regex.Match(fileData.FileName, _regex);

            if (match == null)
                return (false, "No match was found");

            var foundElements = match.Groups
                                     .OfType<Group>()
                                     .Where(g => g.Success)
                                     .ToDictionary(g => g.Name, g => g);

            if (foundElements == null)
                return (false, "ERROR: No successful match groups were found");

            Print.FileData(fileData);

            var updates = new UpdateableFields();
            foreach (var element in foundElements)
            {
                if (element.Value.Name == "Title")
                    updates.Title = element.Value.Value.Trim();
                if (element.Value.Name == "Artists")
                    updates.Artists = element.Value.Value.Split(new [] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (element.Value.Name == "Year")
                    updates.Year = uint.TryParse(element.Value.Value, out var parsed) ? parsed : 0;
                if (element.Value.Name == "Genres")
                    updates.Genres = element.Value.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            }

            Console.WriteLine("Proposed updates:");

            byte proposedUpdateCount = 0;
            if (updates.Title != null && updates.Title != fileData.Title)
            {
                Console.WriteLine("  - Title: " + updates.Title);
                proposedUpdateCount++;
            }
            if (updates.Artists != null && !updates.Artists.All(a => fileData.Artists.Contains(a)))
            {
                Console.WriteLine("  - Artists: " + string.Join(";", updates.Artists));
                proposedUpdateCount++;
            }
            if (updates.Year != null && updates.Year != fileData.Year)
            {
                Console.WriteLine("  - Year: " + updates.Year);
                proposedUpdateCount++;
            }
            if (updates.Genres != null && !updates.Genres.All(a => fileData.Genres.Contains(a)))
            {
                Console.WriteLine("  - Genres: " + string.Join(";", updates.Genres));
                proposedUpdateCount++;
            }

            if (proposedUpdateCount == 0)
            {
                Console.WriteLine("There were no updates found to make!");
                Console.ReadLine();
                return (false, "There were no updates found to make");
            }

            Console.WriteLine("Do you want to apply these updates to the file?");
            Console.Write("Enter Y or N:  ");

            var validInput = false;
            var shouldUpdate = false;
            do
            {
                var reply = Console.ReadKey();
                if (reply.KeyChar == 'n' || reply.KeyChar == 'N' ||
                    reply.KeyChar == 'y' || reply.KeyChar == 'Y')
                {
                    shouldUpdate = reply.KeyChar == 'y' || reply.KeyChar == 'Y';
                    validInput = true;
                }
            }
            while (!validInput);
            Console.WriteLine();

            if (!shouldUpdate)
                return (false, "Updates were cancelled");

            //byte updatesMade = 0;
            if (updates.Title != null && updates.Title != fileData.Title)
            {
                fileData.Title = updates.Title;
                //updatesMade++;
            }                
            if (updates.Artists?.All(a => fileData.Artists.Contains(a)) == false)
            {
                fileData.Artists = updates.Artists;
                //updatesMade++;
            }
            if (updates.Year != null && updates.Year != fileData.Year)
            {
                fileData.Year = updates.Year.Value;
                //updatesMade++;
            }
            if (updates.Genres?.All(a => fileData.Genres.Contains(a)) == false)
            {
                fileData.Genres = updates.Genres;
                //updatesMade++;
            }

            fileData.SaveUpdates();

            return (true, "Updates saved!");
        }
    }
}