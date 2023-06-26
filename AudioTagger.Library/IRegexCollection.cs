using System.Text.RegularExpressions;

namespace AudioTagger;

public interface IRegexCollection
{
    IReadOnlyCollection<string> Patterns { get; }
    Match? GetFirstFileMatch(string fileName);
}
