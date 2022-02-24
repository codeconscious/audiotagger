using System.Text.RegularExpressions;

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
            #region Album-based
            // @"(?<artists>.+?) - (?<album>.+?) - (?:(?<discNo>[1-9]{1,2})[\.-])?(?<trackNo>[0-9]+) - (?<title>.+?(?:\.{3})?)(?: \[(?<year>\d{4})\])?(?: \{(?<genres>.+?)\})?(?=\.\S+)",
            @"(?<artists>.+?) - (?<album>.+?)(?: ?\[(?<year>\d{4})\])? - (?:(?<discNo>[1-9]{1,2})[\.-])?(?<trackNo>[0-9]+) - (?<title>.+?(?:\.{3})?)(?: \{(?<genres>.+?)\})?(?=\.\S+)",
            @"(?<artists>.+?) - (?<album>.+?)(?: ?\[(?<year>\d{4})\])? - (?<title>.+?(?:\.{3})?)(?: \{(?<genres>.+?)\})?(?=\.\S+)",
            // AlbumName = 020 20. TrackName - TrackArtist.mp3 (Special case)
            @"(?<album>.+?)(?: ?\[(?<year>\d{4})\])? = (?:\d{1,3} )?(?:(?<discNo>[1-9]{1,2})[\.-])?(?:(?<trackNo>[0-9]+)\.?)(?<title>.+?(?:\.{3})?) - (?<artists>.+?)(?: \{(?<genres>.+)\})?(?:\..+)",
            @"(?<album>.+?)(?: ?\[(?<year>\d{4})\])? = (?<artists>.+?) - (?<title>.+?(?:\.{3})?)(?: \{(?<genres>.+)\})?(?:\..+)",


            #endregion

            #region Track-based
            @"(?<artists>.+) - (?<album>.+) - (?<discNo>[1-9]{1,2})[\.-](?<trackNo>[1-9]+) - (?<title>.+?(?:\.{3})?) (?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\..+)",
            @"(?<artists>.+) - (?<album>.+) - (?<trackNo>[1-9]{1,3}) - (?<title>.+?(?:\.{3})?) ?(?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\..+)",
            @"(?<artists>.+) - (?<album>.+) - (?<title>.+?(?:\.{3})?) ?(?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\..+)",
            @"(?<artists>.+) - (?<title>.+?(?:\.{3})?)(?: \[(?<year>\d{4})\])?(?: \{(?<genres>.+?)\})?(?=\.\S+)",
            @"(?<title>.+?) ?(?:\[(?<year>\d{4})\])? ?(?:\{(?<genres>.+?)\})?(?=\.[^.]+$)",
            #endregion
        };

        /// <summary>
        /// Returns the first found regex matches for a filename, or null if none.
        /// </summary>
        /// <returns>Matched tag data; otherwise, null if no matches are found.</returns>
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
