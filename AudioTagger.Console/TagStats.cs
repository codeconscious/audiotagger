using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class TagStats : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        const int topArtistCount = 25;
        var ignoreArtists = new string[] { string.Empty, "Various", "Various Artists", "<unknown>" };

        var topArtists = mediaFiles
            .Where(m => m.Artists.Any() &&
                        m.AlbumArtists.All(a => !ignoreArtists.Contains(a)) &&
                        !ignoreArtists.Intersect(m.Artists).Any() &&
                        !m.Genres.Contains("日本語会話"))
            .GroupBy(a => a.AlbumArtists.Any() ? a.AlbumArtists : a.Artists, new ArtistsComparer())
            .ToImmutableDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value)
            .Take(topArtistCount);

        PrintToTable(
            $"Top {topArtistCount} artists:",
            new[] { "Artist", "Count" },
            topArtists.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            new List<Justify>() { Justify.Left, Justify.Right });

        const int mostCommonTitleCount = 15;

        var mostCommonTitles = mediaFiles
            .GroupBy(a => a.Title.Trim(), new CaseInsensitiveStringComparer())
            .ToImmutableDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value)
            .Take(mostCommonTitleCount);

        PrintToTable(
            $"Top {mostCommonTitleCount} track titles:",
            new[] { "Title", "Count" },
            mostCommonTitles.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            new List<Justify>() { Justify.Left, Justify.Right });

        const int mostCommonGenreCount = 20;

        var genresWithCounts = mediaFiles
            .SelectMany(file => file.Genres)
            .GroupBy(g => g.Trim(), new CaseInsensitiveStringComparer())
            .ToImmutableDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value);

        var mostCommonGenres = genresWithCounts.Take(mostCommonGenreCount);

        PrintToTable(
            $"Top {mostCommonGenreCount} genres:",
            new[] { "Genre", "Count" },
            mostCommonGenres.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            new List<Justify>() { Justify.Left, Justify.Right });

        const int leastCommonGenreCount = 10;

        var leastCommonGenres = genresWithCounts.TakeLast(leastCommonGenreCount);

        PrintToTable(
            $"Bottom {leastCommonGenreCount} genres:",
            new[] { "Genre", "Count" },
            leastCommonGenres.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            new List<Justify>() { Justify.Left, Justify.Right });

        const int mostCommonYearCount = 15;

        var mostCommonYears = mediaFiles
            // .Where(f => f.Year != 0)
            .GroupBy(f => f.Year)
            .ToImmutableDictionary(f => f.Key, f => f.Count())
            .OrderByDescending(f => f.Value)
            .Take(mostCommonYearCount);

        PrintToTable(
            $"Top {mostCommonYearCount} Years",
            new[] { "Year", "Count" },
            mostCommonYears.Select(y => new[] { y.Key.ToString(), y.Value.ToString("#,##0") }).ToList(),
            new List<Justify>() { Justify.Left, Justify.Right });

        const int longestTrackCount = 15;
        var longestTracks = mediaFiles
            .OrderByDescending(f => f.Duration)
            .Take(longestTrackCount);

        // Debugging use only // TODO: Delete
        longestTracks.ToList().ForEach(t => System.Console.WriteLine(t.Title));

        PrintToTable(
            $"{longestTrackCount} Longest Tracks",
            new[] { "Artist", "Title", "Duration", "Size" },
            longestTracks.Select(t => new[] { t.Artists.First(), t.Title, t.Duration.ToString() }).ToList(),
            new List<Justify>() { Justify.Left, Justify.Left, Justify.Right });
    }

    private static void PrintToTable(string title,
                                     IList<string> columnNames,
                                     List<string[]> rows,
                                     List<Justify>? justifications = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("A title must be provided.");

        if (columnNames == null || rows == null || !columnNames.Any() || !rows.Any())
            throw new InvalidOperationException("Column names and row data must be provided.");

        if (columnNames.Count != rows[0].Length ||
            !rows.All(r => r.Length == columnNames.Count))
        {
            throw new InvalidOperationException("The counts of columns and rows must be identical.");
        }

        if (justifications != null && justifications.Count != columnNames.Count)
        {
            throw new InvalidOperationException(
                "If justifications are provided, the count must be identical to the column count.");
        }

        var table = new Table
        {
            Border = TableBorder.None
        };
        table.AddColumns(columnNames.Select(n => $"[gray]{n}[/]").ToArray());
        table.Columns[0].Width = rows.Max(r => r[0].Length + 3);
        table.Columns[1].Width = rows.Max(r => Math.Max(r[1].Length + 3, 6));
        if (columnNames.Count > 2)
        {
            table.Columns[2].Width = rows.Max(r => Math.Max(r[1].Length + 3, 6));
        }
        if (justifications != null)
        {
            for (int i = 0; i < justifications.Count; i++)
            {
                table.Columns[i].Alignment = justifications[i];
            }
        }

        rows.ForEach(r => table.AddRow(r));

        var panel = new Panel(table)
        {
            Header = new PanelHeader(title)
        };

        AnsiConsole.Write(panel);
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

    private sealed class CaseInsensitiveStringComparer : IEqualityComparer<string>
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
