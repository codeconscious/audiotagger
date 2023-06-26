using System.Text.RegularExpressions;

namespace AudioTagger;

public class RegexCollection : IRegexCollection
{
    /// <summary>
    /// The regexes used for reading tags from names.
    /// </summary>
    public IReadOnlyCollection<string> Patterns { get; }

    public RegexCollection(IEnumerable<string> regexes)
    {
        if (regexes is null)
            throw new ArgumentNullException(nameof(regexes));

        if (!regexes.Any())
            throw new InvalidOperationException("No regex patterns were found.");

        Patterns = regexes.Where(line =>
                               !line.StartsWith("# ") &&  // Comments
                               !line.StartsWith("// ") &&  // Comments
                               !string.IsNullOrWhiteSpace(line))
                          .Distinct()
                          .ToList();
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
