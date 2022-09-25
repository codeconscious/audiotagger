using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public class TagDuplicateFinder : IPathOperation
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

        printer.Print("Checking for duplicates (by artist(s) and title)...");

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        var duplicateGroups = mediaFiles
            .ToLookup(m => ConcatenateArtists(m.Artists) + m.Title.Trim())
            .Where(m => !string.IsNullOrWhiteSpace(m.Key) && m.Count() > 1)
            .OrderBy(m => m.Key);

        var count = duplicateGroups.Count();

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        printer.Print($"Found {count} duplicate{(count == 1 ? "" : "s")} in {elapsedMs:#,##0}ms.");

        uint index = 0;
        int indexPadding = count.ToString().Length + 2;

        foreach (var dupeGroup in duplicateGroups)
        {
            index++;
            var firstDupe = dupeGroup.First();
            var artist = string.Concat(firstDupe.Artists);
            var title = firstDupe.Title;
            var bitrate = "(" + string.Join(", ", dupeGroup.Select(d => d.BitRate + "kpbs")) + ")";
            printer.Print($"{index.ToString().PadLeft(indexPadding)}. {artist} / {title}  {bitrate}");
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
