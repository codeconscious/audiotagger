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

        var titleReplacements = settings?.Duplicates?.TitleReplacements ??
                                ImmutableList<string>.Empty;
        printer.Print($"Found {titleReplacements.Count} replacement term(s).");

        printer.Print("Checking for duplicates by artist(s) and title...");

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        var duplicateGroups = mediaFiles
            .ToLookup(m => ConcatenateArtistsForComparison(m.Artists) +
                           RemoveUnneededText(m.Title, titleReplacements))
            .Where(m => !string.IsNullOrWhiteSpace(m.Key) && m.Count() > 1)
            .OrderBy(m => m.Key)
            .ToImmutableArray();

        var count = duplicateGroups.Length;

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

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
                var header = innerIndex == 0
                    ? groupIndex.ToString().PadLeft(groupIndexPadding) + groupIndexAppend
                    : new string(' ', groupIndexPadding + groupIndexAppend.Length);
                var titleArtist = SummarizeArtistTitle(mediaFile);
                var titleArtistFormatted = new LineSubString(header + titleArtist);
                var separator = new LineSubString(new string(' ', longestTitleLength - titleArtist.Length));
                var metadata = new LineSubString(
                    "  " + SummarizeMetadata(mediaFile) + Environment.NewLine, // TODO: Make the new line unnecessary.
                    ConsoleColor.Gray // TODO: Figure out why this doesn't work.
                );
                printer.Print(new[] { titleArtistFormatted, separator, metadata});

                innerIndex++;
            }

            groupIndex++;
            innerIndex = 0;
        }

        static string SummarizeArtistTitle(MediaFile mediaFile)
        {
            var artist = string.Join("; ", mediaFile.Artists);
            var title = mediaFile.Title;
            return $"{artist} / {title}";
        }

        static string SummarizeMetadata(MediaFile mediaFile)
        {
            var ext = Path.GetExtension(mediaFile.Path).ToUpperInvariant();
            var bitrate = mediaFile.BitRate + " kpbs";
            var fileSize = mediaFile.FileSizeInBytes.ToString("#,##0") + " bytes";

            var minutes = Math.Floor(mediaFile.Duration.TotalSeconds / 60);
            var seconds = Math.Ceiling(mediaFile.Duration.TotalSeconds % 60);
            var time = $"{minutes}:{seconds:00}";

            return $"({ext[1..]}; {bitrate}; {fileSize}; {time})";
        }
    }

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
    /// Removes each occurrence of text using a given collection of strings.
    /// </summary>
    /// <returns>The modified string.</returns>
    private static string RemoveUnneededText(string title, ImmutableList<string> terms)
    {
        return terms switch
        {
            null         => title,
            { Count: 0 } => title,
            _ => terms.ToList()
                      .Aggregate(
                          new StringBuilder(title),
                          (sb, term) => sb.Replace(term, string.Empty),
                          sb => sb.ToString().Trim())
        };
    }
}
