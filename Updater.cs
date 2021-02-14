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

        public static string UpdateTags(FileData fileData, DataPrinter printer)
        {
            var regex = new Regex(_regex);

            var match = Regex.Match(fileData.FileName, _regex);

            if (match == null)
                return "ERROR: No match was found.";

            var foundElements = match.Groups
                                     .OfType<Group>()
                                     .Where(g => g.Success)
                                     .ToDictionary(g => g.Name, g => g);

            if (foundElements == null)
                return "ERROR: No successful matches were found";

            printer.PrintData(fileData);

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

            if (updates.Title != null && updates.Title != fileData.Title)
                Console.WriteLine("  - Title: " + updates.Title);
            if (updates.Artists != null && !updates.Artists.All(a => fileData.Artists.Contains(a)))
                Console.WriteLine("  - Artists: " + string.Join(";", updates.Artists));
            if (updates.Year != null && updates.Year != fileData.Year)
                Console.WriteLine("  - Year: " + updates.Year);
            if (updates.Genres != null && !updates.Genres.All(a => fileData.Genres.Contains(a)))
                Console.WriteLine("  - Genres: " + string.Join(";", updates.Genres));

            Console.WriteLine("Do you want to apply these updates to the file?");
            Console.Write("Enter Y or N:  ");

            var validInput = false;
            var shouldUpdate = false;
            do
            {
                var reply = Console.ReadKey();
                if (reply.KeyChar == 'n' || reply.KeyChar == 'y')
                {
                    shouldUpdate = reply.KeyChar == 'y';
                    validInput = true;
                }
            }
            while (!validInput);
            Console.WriteLine();

            if (!shouldUpdate)
                return "Updates cancelled.";

            if (updates.Title != null && updates.Title != fileData.Title)
                fileData.Title = updates.Title;
            if (updates.Artists?.All(a => fileData.Artists.Contains(a)) == false)
                fileData.Artists = updates.Artists;
            if (updates.Year != null && updates.Year != fileData.Year)
                fileData.Year = updates.Year.Value;
            if (updates.Genres?.All(a => fileData.Genres.Contains(a)) == false)
                fileData.Genres = updates.Genres;

            fileData.SaveUpdates();

            return "Updates saved!";
        }
    }
}