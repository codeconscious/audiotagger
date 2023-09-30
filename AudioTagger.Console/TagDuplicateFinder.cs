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
            .ToLookup(m => ConcatenateArtists(m.Artists) +
                           RemoveUnneededText(m.Title, titleReplacements))
            .Where(m => !string.IsNullOrWhiteSpace(m.Key) && m.Count() > 1)
            .OrderBy(m => m.Key)
            .ToImmutableArray();

        var count = duplicateGroups.Length;

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        printer.Print($"Found {count} duplicate{(count == 1 ? "" : "s")} in {elapsedMs:#,##0}ms.");

        PrintResults(duplicateGroups, printer);
    }

    private static void PrintResults(IList<IGrouping<string, MediaFile>> duplicateGroups, IPrinter printer)
    {
        int groupIndex = 1;
        int groupIndexPadding = duplicateGroups.Count.ToString().Length + 2;
        int innerIndex = 0;

        foreach (IGrouping<string, MediaFile> dupeGroup in duplicateGroups)
        {
            foreach (MediaFile mediaFile in dupeGroup)
            {
                var header = innerIndex == 0
                    ? groupIndex.ToString().PadLeft(groupIndexPadding) + ". "
                    : new string(' ', groupIndexPadding + 2); // 2 for the length of ". "
                printer.Print(header + SummarizeMediaFile(mediaFile));
                innerIndex++;
            }

            groupIndex++;
            innerIndex = 0;
        }

        static string SummarizeMediaFile(MediaFile mediaFile)
        {
            var artist = string.Join("; ", mediaFile.Artists);
            var title = mediaFile.Title;
            var ext = Path.GetExtension(mediaFile.Path).ToUpperInvariant();
            var bitrate = mediaFile.BitRate + "kpbs";
            return $"{artist} / {title}  ({ext[1..]}, {bitrate})";
        }
    }

    private static string ConcatenateArtists(IEnumerable<string> artists)
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
