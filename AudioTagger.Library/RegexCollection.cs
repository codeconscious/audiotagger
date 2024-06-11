using System.Text.RegularExpressions;
using AudioTagger.Library;

namespace AudioTagger;

public sealed record class RegexCollection
{
    /// <summary>
    /// The regexes used for reading tags from names.
    /// </summary>
    public IReadOnlyCollection<string> Patterns { get; }

    public RegexCollection(IEnumerable<string> regexes)
    {
        ArgumentNullException.ThrowIfNull(regexes);

        if (!regexes.Any())
        {
            throw new InvalidOperationException("No regex patterns were found.");
        }

        Patterns = regexes.Where(line =>
                               !line.StartsWith("# ") &&  // Comments
                               !line.StartsWith("// ") && // Comments
                               line.HasText())
                          .Distinct()
                          .ToList();
    }
}

public static class RegexCollectionExtensionMethods
{
    /// <summary>
    /// Returns the first found regex matches for a filename, or null if none.
    /// </summary>
    /// <returns>Matched tag data; otherwise, null if no matches are found.</returns>
    public static Match? GetFirstMatch(this RegexCollection regexes,
                                       string fileName)
    {
        foreach (string pattern in regexes.Patterns)
        {
            Match match = Regex.Match(fileName, pattern, RegexOptions.CultureInvariant);

            if (match.Success)
            {
                return match;
            }
        }

        return null;
    }
}
