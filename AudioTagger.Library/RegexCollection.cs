using System.Text.RegularExpressions;

namespace AudioTagger
{
    public static class RegexCollection
    {
        // TODO: Place these into a file instead.
        /// <summary>
        /// The regexes used for reading tags from names.
        /// </summary>
        public static List<string> Regexes => new()
        {
            // Archival pattern (Example: 《Various Artists》〈岩崎良美〉『LOVE ALBUM』［1.12］｛1979｝「紫の恋を」〔J-Pop〕【Japan】.mp3)
            // @"(?:(?:《(?<albumartists>.+?)》)?|(?:〈(?<artists>.+?)〉)?|(?:『(?<album>.+)』)?|(?:［(?:(?<disc>\d+)?\.)?(?<track>\d+)］)?|(?:(?<!_)「(?<title>紫の恋を)」(?!_))?|(?:｛(?<year>.+)｝)?|(?:〔(?<genres>.+)〕)?|(?:【(?<country>.*)】)?)*\.(?<extension>.+)",

            #region YouTube-based
            @"(?<artists>.+?)(?:『(?<album>.+)』)?(?:#(?:(?<track>\d+)))?「(?<title>.+?)」(?:\[(?<year>\d{4})\])?",
            // @"(?<artists>.+)「(?<title>.+)」\[(?<year>\d{4})\](?= \[.{11}\])",
            // @"(?<albumArtists>.+) ≡ (?<album>.+) = (?<trackNo>\d+) - (?<artists>.+) – (?<title>.+)",
            #endregion

            #region Album-based
            @"(?:(?<albumArtists>.+) ≡ )?(?<album>.+?)(?: ?\[(?<year>\d{4})\])? = (?<trackNo>\d+) - (?<artists>.+?) [–-] (?<title>.+)(?=\.mp3)",
            @"(?:(?<albumArtists>.+) ≡ )?(?<album>.+?)(?: ?\[(?<year>\d{4})\])? = (?<trackNo>\d{1,3}) [–-] (?<title>.+)(?=\.mp3)",
            @"(?:(?<albumArtists>.+) ≡ )(?<album>.+?)(?: ?\[(?<year>\d{4})\])? = (?<artists>.+?) [–-] (?<title>.+)(?=\.mp3)",
            // @"(?<artists>.+?) - (?<album>.+?) - (?:(?<discNo>[1-9]{1,2})[\.-])?(?<trackNo>[0-9]+) - (?<title>.+?(?:\.{3})?)(?: \[(?<year>\d{4})\])?(?: \{(?<genres>.+?)\})?(?=\.\S+)",
            @"(?<artists>.+?) - (?<album>.+?)(?: ?\[(?<year>\d{4})\])? - (?:(?<discNo>[1-9]{1,2})[\.-])?(?<trackNo>[0-9]+) - (?<title>.+?(?:\.{3})?)(?: \{(?<genres>.+?)\})?(?=\.\S+)",
            @"(?<artists>.+?) - (?<album>.+?)(?: ?\[(?<year>\d{4})\])? - (?<title>.+?(?:\.{3})?)(?: \{(?<genres>.+?)\})?(?=\.\S+)",
            // AlbumName = 020 20. TrackName - TrackArtist.mp3 (Special case)
            @"(?<album>.+?)(?: ?\[(?<year>\d{4})\])? = (?:\d{1,3} )?(?:(?<discNo>[1-9]{1,2})[\.-])?(?:(?<trackNo>[0-9]+)\.?)?(?<title>.+?(?:\.{3})?)(?: \{(?<genres>.+)\})?(?:\..+)",
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
