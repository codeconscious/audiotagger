using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public class SpecificTagUpdater : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles, DirectoryInfo workingDirectory, IPrinter printer)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        const string pattern = @"F\d\d";
        var updateFiles = mediaFiles.Where(f => Regex.IsMatch(f.Album, pattern));

        printer.Print("Updating album tags...");

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        foreach (var file in updateFiles)
        {
        }

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        // printer.Print($"Found {count} duplicate{(count == 1 ? "" : "s")} in {elapsedMs:#,##0}ms.");

        // uint index = 0;
        // int indexPadding = count.ToString().Length + 2;
    }
}
