using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public class TagStats : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles, DirectoryInfo workingDirectory, IPrinter printer)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

         var artistStats = mediaFiles
            .Where(m =>
                m.Artists.Any() &&
                string.Concat(m.Artists) != "<Unknown>" && // Not working.
                !m.Genres.Contains("日本語会話"))
            .GroupBy(a => a.Artists, new ArtistsComparer())
            .ToImmutableDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value)
            .Take(30);

        foreach (var artist in artistStats)
            printer.Print($"  - {artist.Key}: {artist.Value}");
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
}
