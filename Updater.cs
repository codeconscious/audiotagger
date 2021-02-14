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
        private const string _regex = @"(?:(?'Artists'.+) \- )?(?'Title'[^\[]+)\s?(?:\[(?'Year'\d{3,})\])?\s?(?:\{(?'Genre'.+)\})";

        public static string UpdateTags(FileData fileData)
        {
            var regex = new Regex(_regex);

            var match = Regex.Match(fileData.FileName, _regex);

            if (match == null)
                return "ERROR: No match was found.";

            var foundElements = match.Groups.OfType<Group>().Where(g => g.Success).ToDictionary(g => g.Name, g => g);

            if (foundElements == null)
                return "ERROR: No successful matches were found";

            // TODO: COMPLETE

            return "Success!";
        }
    }
}