using System.Text.RegularExpressions;
using System.Text;

namespace AudioTagger.Console;

public sealed class TagDuplicateFinder : IPathOperation
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

        ImmutableList<string> titleReplacements = settings?.Duplicates?.TitleReplacements ??
                                                  ImmutableList<string>.Empty;
        printer.Print($"Found {titleReplacements.Count} replacement term(s).");

        printer.Print("Checking for duplicates by artist(s) and title...");

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        var duplicateGroups = mediaFiles
            .ToLookup(m => ConcatenateArtistsForComparison(m.Artists) +
                           RemoveSubstrings(m.Title, titleReplacements))
            .Where(m => !string.IsNullOrWhiteSpace(m.Key) && m.Count() > 1)
            .OrderBy(m => m.Key)
            .ToImmutableArray();

        int count = duplicateGroups.Length;

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        double elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        printer.Print($"Found {count} duplicate group{(count == 1 ? "" : "s")} in {elapsedMs:#,##0}ms.");

        PrintResults(duplicateGroups, printer);
    }

    private static void PrintResults(IList<IGrouping<string, MediaFile>> duplicateGroups, IPrinter printer)
    {
        int groupIndex = 1;
        int groupIndexPadding = duplicateGroups.Count.ToString().Length + 2;
        int innerIndex = 0;
        const string groupIndexAppend = ". ";

        foreach (IGrouping<string, MediaFile> dupeGroup in duplicateGroups)
        {
            int longestTitleLength = dupeGroup.Max(file => SummarizeArtistTitle(file).Length);

            foreach (MediaFile mediaFile in dupeGroup)
            {
                string header = innerIndex == 0
                    ? groupIndex.ToString().PadLeft(groupIndexPadding) + groupIndexAppend
                    : new string(' ', groupIndexPadding + groupIndexAppend.Length);
                string titleArtist = SummarizeArtistTitle(mediaFile);
                LineSubString titleArtistFormatted = new(header + titleArtist);
                LineSubString separator = new(new string(' ', longestTitleLength - titleArtist.Length + 1));
                LineSubString metadata = new(
                    text: "  " + SummarizeMetadata(mediaFile),
                    fgColor: ConsoleColor.Cyan,
                    bgColor: null,
                    addLineBreak: true
                );
                printer.Print(new LineSubString[] { titleArtistFormatted, separator, metadata });

                innerIndex++;
            }

            groupIndex++;
            innerIndex = 0;
        }

        static string SummarizeArtistTitle(MediaFile mediaFile)
        {
            string artist = string.Join("; ", mediaFile.Artists);
            string title = mediaFile.Title;
            return $"{artist}  /  {title}";
        }

        static string SummarizeMetadata(MediaFile mediaFile)
        {
            string ext = Path.GetExtension(mediaFile.Path).ToUpperInvariant();
            string bitrate = mediaFile.BitRate + " kpbs";
            string fileSize = mediaFile.FileSizeInBytes.ToString("#,##0") + " bytes";

            double minutes = Math.Floor(mediaFile.Duration.TotalSeconds / 60);
            double seconds = Math.Ceiling(mediaFile.Duration.TotalSeconds % 60);
            string time = $"{minutes}:{seconds:00}";

            return $"({ext[1..]}; {bitrate}; {time}; {fileSize})";
        }
    }

    /// <summary>
    /// Removes parts of a string that should be ignored for comparison.
    /// For example, "The Beatles" would convert to "Beatles" because "The"
    /// should not be included in the comparison.
    /// </summary>
    /// <param name="artists"></param>
    /// <returns></returns>
    private static string ConcatenateArtistsForComparison(IEnumerable<string> artists)
    {
        return
            Regex.Replace(
                string.Concat(artists)
                      .ToLowerInvariant()
                      .Trim(),
                "^the",
                "");
    }

    /// <summary>
    /// Removes occurrences of each of a collection of substrings from a string.
    /// Returns the source string as-is if no replacement terms were passed in.
    /// </summary>
    /// <returns>The modified string.</returns>
    private static string RemoveSubstrings(string source, ImmutableList<string> terms)
    {
        return terms switch
        {
            null or { Count: 0 } => source,
            _                    => terms.ToList()
                                         .Aggregate(
                                             new StringBuilder(source),
                                             (sb, term) => sb.Replace(term, string.Empty),
                                             sb => sb.ToString())
        };
    }
}
