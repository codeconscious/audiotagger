using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AudioTagger
{
    public class RegexCollection
    {
        public List<string> Regexes { get; private set; }

        public RegexCollection(string sourceFile)
        {
            // Check if file exists
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"\"{nameof(sourceFile)}\" missing", sourceFile);

            var fileLines = File.ReadAllLines(sourceFile, Encoding.UTF8)
                            ?? Array.Empty<string>();

            if (!fileLines.Any())
                throw new Exception($"No regexes were found in file {sourceFile}");

            Regexes = fileLines.Distinct().ToList();
        }

        /// <summary>
        /// Returns the first found regex matches for a filename, or null if none.
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public Match? GetFirstMatch(FileData fileData)
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
