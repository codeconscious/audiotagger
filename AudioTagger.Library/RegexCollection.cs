using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Configuration;

namespace AudioTagger
{
    public static class RegexCollection
    {
        /// <summary>
        /// The regexes used for reading tags from names.
        /// </summary>
        public static List<string> Regexes => new()
        {
            /// TODO: Place into a file instead.
            @"(?<artists>.+) - (?<album>.+) - (?<discNo>[1-9]{1,2})[\.-](?<trackNo>[1-9]+) - (?<title>.+?) (?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\..+)",
            @"(?<artists>.+) - (?<album>.+) - (?<trackNo>[1-9]{1,3}) - (?<title>.+?) ?(?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\..+)",
            @"(?<artists>.+) - (?<album>.+) - (?<title>.+?) ?(?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\..+)",
            @"(?<artists>.+) - (?<title>.+?) ?(?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\..+)",
            @"(?<title>.+?) ?(?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\.[^.]+$)"
        };

        /// <summary>
        /// Returns the first found regex matches for a filename, or null if none.
        /// </summary>
        /// <returns>Matched tag data</returns>
        public static Match? GetFirstMatch(string fileName)
        {
            foreach (var regexText in Regexes)
            {
                var match = Regex.Match(fileName,
                                        regexText,
                                        RegexOptions.CultureInvariant);

                if (match.Success)
                    return match;
            }

            return null;
        }
    }
}
