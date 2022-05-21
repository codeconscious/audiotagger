using System.IO;
using System.Text.RegularExpressions;

namespace AudioTagger;

public interface IRegexCollection
{
    IReadOnlyCollection<string> Patterns { get; }
    Match? GetFirstFileMatch(string fileName);
}

public class RegexCollection : IRegexCollection
{
    /// <summary>
    /// The regexes used for reading tags from names.
    /// </summary>
    public IReadOnlyCollection<string> Patterns { get; }

    public RegexCollection(string regexFileName)
    {
        if (string.IsNullOrWhiteSpace(regexFileName))
            throw new ArgumentNullException(nameof(regexFileName));

        if (!File.Exists(regexFileName))
            throw new FileNotFoundException($"The regex file \"{regexFileName}\" was not found.");

        var patterns = File.ReadAllLines(regexFileName)
                           .Where(line =>
                                !line.StartsWith("# ") &&  // Comments
                                !string.IsNullOrWhiteSpace(line))
                           .Distinct()
                           .ToList();

        if (patterns.Count == 0)
            throw new InvalidDataException("No regex patterns were found.");

        Patterns = patterns;
    }

    /// <summary>
    /// Returns the first found regex matches for a filename, or null if none.
    /// </summary>
    /// <returns>Matched tag data; otherwise, null if no matches are found.</returns>
    public Match? GetFirstFileMatch(string fileName)
    {
        foreach (var pattern in Patterns)
        {
            var match = Regex.Match(fileName,
                                    pattern,
                                    RegexOptions.CultureInvariant);

            if (match.Success)
                return match;
        }

        return null;
    }
}
