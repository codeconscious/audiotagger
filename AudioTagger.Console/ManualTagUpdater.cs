using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public class ManualTagUpdater : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles, DirectoryInfo workingDirectory, IPrinter printer)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        const string pattern = @"TEST ALBUM NAME";
        var updateFiles = mediaFiles.Where(f => Regex.IsMatch(f.Album.Trim(), pattern, RegexOptions.IgnoreCase));

        if (!updateFiles.Any())
        {
            printer.Print("No matching files were found! Cancelling...");
            return;
        }

        printer.Print($"Updating {updateFiles.Count()} tags...");

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        uint successCount = 0;
        uint failureCount = 0;
        const string outputPrepend = "  - ";

        foreach (var file in updateFiles)
        {
            try
            {
                file.Album = string.Empty;
                file.SaveUpdates();
                successCount++;
                //printer.Print(outputPrepend + $"OK: {file.Path}");
            }
            catch
            {
                failureCount++;
                printer.Print(outputPrepend + $"ERROR: {file.Path}");
            }
        }

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        printer.Print($"Done in {elapsedMs:#,##0}ms -- {successCount} successes, {failureCount} failures");
    }
}
