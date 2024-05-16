using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class TagStats : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        const int topArtistCount = 25;
        string[] ignoreArtists = [string.Empty, "VA", "Various", "Various Artists", "<unknown>"];

        var topArtists = mediaFiles
            .Where(m => m.Artists.Any() &&
                        m.AlbumArtists.All(a => !ignoreArtists.Contains(a)) &&
                        !ignoreArtists.Intersect(m.Artists).Any() &&
                        !m.Genres.Contains("日本語会話"))
            .GroupBy(a =>
                a.AlbumArtists.Any()
                    ? a.AlbumArtists
                    : a.Artists, new ArtistsComparer())
            .ToDictionary(g =>
                string.Join(", ", g.Key),
                g => g.Count())
            .OrderByDescending(g => g.Value)
            .Take(topArtistCount);

        PrintToTable(
            $"Top {topArtistCount} artists:",
            ["Artist", "Count"],
            topArtists.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            [Justify.Left, Justify.Right]);

        const int mostCommonTitleCount = 15;

        var mostCommonTitles = mediaFiles
            .GroupBy(a => a.Title.Trim(), new CaseInsensitiveStringComparer())
            .ToDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value)
            .Take(mostCommonTitleCount);

        PrintToTable(
            $"Top {mostCommonTitleCount} track titles:",
            ["Title", "Count"],
            mostCommonTitles.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            [Justify.Left, Justify.Right]);

        const int mostCommonGenreCount = 20;

        var genresWithCounts = mediaFiles
            .SelectMany(file => file.Genres)
            .GroupBy(g => g.Trim(), new CaseInsensitiveStringComparer())
            .ToDictionary(g => string.Join(", ", g.Key), g => g.Count())
            .OrderByDescending(g => g.Value);

        var mostCommonGenres = genresWithCounts.Take(mostCommonGenreCount);

        PrintToTable(
            $"Top {mostCommonGenreCount} genres:",
            ["Genre", "Count"],
            mostCommonGenres.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            [Justify.Left, Justify.Right]);

        const int leastCommonGenreCount = 10;

        var leastCommonGenres = genresWithCounts.TakeLast(leastCommonGenreCount);

        PrintToTable(
            $"Bottom {leastCommonGenreCount} genres:",
            ["Genre", "Count"],
            leastCommonGenres.Select(y => new[] { y.Key, y.Value.ToString("#,##0") }).ToList(),
            [Justify.Left, Justify.Right]);

        const int mostCommonYearCount = 15;

        var mostCommonYears = mediaFiles
            // .Where(f => f.Year != 0)
            .GroupBy(f => f.Year)
            .ToDictionary(f => f.Key, f => f.Count())
            .OrderByDescending(f => f.Value)
            .Take(mostCommonYearCount);

        PrintToTable(
            $"Top {mostCommonYearCount} Years",
            ["Year", "Count"],
            mostCommonYears.Select(y => new[] { y.Key.ToString(), y.Value.ToString("#,##0") }).ToList(),
            [Justify.Left, Justify.Right]);

        const int longestTrackCount = 25;
        var longestTracks = mediaFiles
            .OrderByDescending(f => f.Duration)
            .Take(longestTrackCount);

        PrintToTable(
            $"{longestTrackCount} Longest Tracks",
            ["Artist", "Title", "Duration", "Format", "Size (bytes)"],
            longestTracks.Select(t => new[] {
                t.Artists.Any() ? t.Artists.First() : "(Unknown Artist)",
                t.Title,
                string.Format("{0:D2}:{1:D2}", (int) t.Duration.TotalMinutes, t.Duration.Seconds), // mm:ss
                Path.GetExtension(t.FileNameOnly),
                $"{t.FileSizeInBytes:#,##0}"
            }).ToList(),
            [Justify.Left, Justify.Left, Justify.Right, Justify.Right, Justify.Right]);

        const int mostAlbumTracksCount = 30;
        var mostAlbumTracks = mediaFiles
            .Where(m => m.AlbumArt is not null && m.AlbumArt.Length != 0)
            .GroupBy(m => $"{m.ArtistSummary}  /  {m.Album}")
            .ToDictionary(g => g.Key, m => m.Count())
            .OrderByDescending(d => d.Value)
            .Take(mostAlbumTracksCount);

        PrintToTable(
            $"{mostAlbumTracksCount} Albums With Album Art and With The Most Tracks",
             ["Artist & Album", "Tracks"],
            mostAlbumTracks.Select(t => new[] {
                t.Key,
                t.Value.ToString()
            }).ToList(),
            [Justify.Left, Justify.Left]);


        int largestEmbeddedAlbumArtCount = 50;
        var largestEmbeddedAlbumArt = mediaFiles
            .Where(m => m.AlbumArt is not null && m.AlbumArt.Length != 0)
            .OrderByDescending(m => m.AlbumArt.Length)
            .GroupBy(m => $"{m.ArtistSummary}  /  {m.Album}")
            .ToDictionary(g => g.Key, m => (m.Sum(n => n.AlbumArt.Length), m.Count()))
            .OrderByDescending(d => d.Value.Item1)
            .Take(largestEmbeddedAlbumArtCount);

        PrintToTable(
            $"{largestEmbeddedAlbumArtCount} Unique Artists and Albums With The Largest Embedded Album Art",
             ["Artist & Album", "Size (Bytes)", "Count"],
            largestEmbeddedAlbumArt.Select(r => new[] {
                r.Key,
                r.Value.Item1.ToString("#,##0"),
                r.Value.Item2.ToString("#,##0"),
            }).ToList(),
            [Justify.Left, Justify.Right, Justify.Right]);
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
        // table.Columns[0].Width = rows.Max(r => r[0].Length + 3);
        // table.Columns[1].Width = rows.Max(r => Math.Max(r[1].Length + 3, 6));
        // if (columnNames.Count > 2)
        // {
        //     table.Columns[2].Width = rows.Max(r => Math.Max(r[1].Length + 3, 6));
        // }
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
                string.Empty);
        }
    }

    private sealed class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            return (x, y) switch
            {
                (null, null)           => true,
                (null, _) or (_, null) => false,
                _ => string.Equals(x.Trim(), y.Trim(), StringComparison.OrdinalIgnoreCase)
            };
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.ToLower().GetHashCode();
        }
    }
}
