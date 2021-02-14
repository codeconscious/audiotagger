using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AudioTagger
{
    public enum Mode { Read, Update }

    public static class Updater
    {
        private const string _regex = @"(?'Artists'.+) [-–] (?'Title'[^\[\{]+)(?: ?\[(?'Year'\d{3,})\])?(?: ?\{(?'Genres'.+)\})?";

        public static string UpdateTags(FileData fileData, DataPrinter printer)
        {
            var regex = new Regex(_regex);

            var match = Regex.Match(fileData.FileName, _regex);

            if (match == null)
                return "ERROR: No match was found.";

            var foundElements = match.Groups.OfType<Group>().Where(g => g.Success).ToDictionary(g => g.Name, g => g);

            if (foundElements == null)
                return "ERROR: No successful matches were found";

            printer.PrintData(fileData);

            var newFileData = fileData;
            foreach (var element in foundElements)
            {
                if (element.Value.Name == "Title")
                    newFileData = newFileData with { Title = element.Value.Value };
                if (element.Value.Name == "Artists")
                    newFileData = newFileData with { Artists = element.Value.Value.Split(new [] { ";" }, StringSplitOptions.RemoveEmptyEntries) };
                if (element.Value.Name == "Year")
                    newFileData = newFileData with { Year = (uint.TryParse(element.Value.Value, out var parsed) ? parsed : 0) };
                if (element.Value.Name == "Genres")
                    newFileData = newFileData with { Genres = element.Value.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries) };
            }

            Console.WriteLine("NEW DATA:");
            printer.PrintData(newFileData);

            return "Success!";
        }
    }
}