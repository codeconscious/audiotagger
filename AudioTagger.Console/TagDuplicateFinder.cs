using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public class TagDuplicateFinder : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles, DirectoryInfo workingDirectory, IPrinter printer)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        printer.Print("Checking for duplicates (by artist(s) and title)...");

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        var duplicates = mediaFiles
            .ToLookup(m => string.Concat(m.Artists).ToLowerInvariant().Trim() +
                           m.Title.Trim())
            .Where(m => m.Count() > 1)
            .OrderBy(m => m.Key);

        var count = duplicates.Count();

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        printer.Print($"Found {count} duplicate{(count == 1 ? "" : "s")} in {elapsedMs:#,##0}ms.");

        uint index = 0;
        int indexPadding = count.ToString().Length + 2;

        foreach (var dupe in duplicates)
        {
            index++;
            var artist = string.Concat(dupe.First().Artists);
            var title = dupe.First().Title;
            printer.Print($"{index.ToString().PadLeft(indexPadding)}. {artist} / {title}");
        }
    }
}
