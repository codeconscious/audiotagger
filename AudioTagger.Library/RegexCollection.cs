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
        // The regexes used for reading tags from names. Ultimately, this should be in a setting or file.
        public static List<string> Regexes => new()
        {
            @"(?:^(?'artists'.+?) - ?)? ?(?:(?'album'.+?) - ?)?(?:(?'trackNo'\d+?) - ?)? (?:(?'title'.+?[^\[\{]))(?: ?\[(?'year'\d{3,})\])?(?: ?\{(?'genres'.+?)\})?(?=\..+)",
            @"(?'artists'.+) [-–] (?'title'[^\[\{]+)(?: ?\[(?'year'\d{3,})\])?(?: ?\{(?'genres'.+)\})?"
        };

        /// <summary>
        /// Returns the first found regex matches for a filename, or null if none.
        /// </summary>
        /// <returns></returns>
        public static Match? GetFirstMatch(AudioFile fileData)
        {
            foreach (var regexText in Regexes)
            {
                var match = Regex.Match(fileData.FileNameOnly,
                                        regexText,
                                        RegexOptions.CultureInvariant);

                if (match != null)
                    return match;

                continue;
            }

            return null;
        }
    }
}
