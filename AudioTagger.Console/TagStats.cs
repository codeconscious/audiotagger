using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public class TagStats : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IRegexCollection regexCollection,
                      IPrinter printer)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        const int topArtistCount = 25;

        var topArtists = mediaFiles
            .Where(m =>
                m.Artists.Any() &&
                string.Concat(m.Artists) != "<Unknown>" && // Not working.
                !m.Genres.Contains("日本語会話"))
            .GroupBy(a => a.Artists, new ArtistsComparer())
            .ToImmutableDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value)
            .Take(topArtistCount);

        printer.Print($"Top {topArtistCount} artists:", prependLines: 1);
        foreach (var artist in topArtists)
            printer.Print($"  - {artist.Key}: {artist.Value}");

        const int mostCommonTitleCount = 10;

        var mostCommonTitles = mediaFiles
            .GroupBy(a => a.Title.Trim(), new TitleComparer())
            .ToImmutableDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value)
            .Take(mostCommonTitleCount);

        printer.Print($"Top {mostCommonTitleCount} track titles:", prependLines: 1);
        foreach (var title in mostCommonTitles)
            printer.Print($"  - {title.Key}: {title.Value}");
    }

    class ArtistsComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[]? x, string[]? y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return ConcatenateArtists(x) == ConcatenateArtists(y);
        }

        public int GetHashCode([DisallowNull] string[] obj)
        {
            // Not sure this is correct.
            return string.Concat(obj).ToLower().GetHashCode();
        }

        static string ConcatenateArtists(IEnumerable<string> artists)
        {
            return Regex.Replace(
                string.Concat(artists)
                      .ToLowerInvariant()
                      .Trim(),
                "^the",
                "");
        }
    }

    private class TitleComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return string.Equals(x.Trim(), y.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.ToLower().GetHashCode();
        }
    }
}
