using Spectre.Console;

namespace AudioTagger.Console;

/// <summary>
/// Reverse the order of track numbers in the provided media files.
/// </summary>
public sealed class TagUpdaterReverseTrackNumbers : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        if (mediaFiles.Any(m => m.TrackNo is 0))
        {
            printer.Error("At least one file has no track number, so cannot continue.");
            return;
        }

        var sortedFiles = mediaFiles.OrderBy(f => $"{f.TrackNo:00000}{f.Title}")
                                    .ToList();

        printer.Print($"Will update the track number tag of {sortedFiles.Count} file(s):");
        sortedFiles.ForEach(f => printer.Print($"- {f.Path}"));

        var reversedTrackNos = sortedFiles.Select(f => f.TrackNo).Reverse().ToImmutableList();
        var filesWithNewTrackNos = sortedFiles.Zip(reversedTrackNos,
                                                   (file, trackNo) => (File: file, NewTrackNo: trackNo));

        // Display a preview first.
        Table table = new();
        table.AddColumns("Filename", "Current", "Proposed");
        foreach ((MediaFile File, uint NewTrackNo) pair in filesWithNewTrackNos)
        {
            table.AddRow(
                Markup.Escape(pair.File.FileNameOnly),
                pair.File.TrackNo.ToString(),
                pair.NewTrackNo.ToString());
        }
        AnsiConsole.Write(table);

        if (!ConfirmContinue())
        {
            printer.Print("Cancelling!");
            return;
        }

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        uint successCount = 0;
        uint failureCount = 0;

        foreach ((MediaFile File, uint NewTrackNo) pair in filesWithNewTrackNos)
        {
            try
            {
                pair.File.TrackNo = pair.NewTrackNo;
                pair.File.SaveUpdates();
                successCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                printer.Error($"Error for \"{pair.File.Path}\": {ex.Message}");
            }
        }

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        double elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        string successLabel = successCount == 1 ? "success" : "successes";
        string failureLabel = failureCount == 1 ? "failure" : "failures";
        printer.Print($"Done in {elapsedMs:#,##0}ms with {successCount} {successLabel} and {failureCount} {failureLabel}.");
    }

    private static bool ConfirmContinue()
    {
        const string no = "No!";
        const string yes = "Yes";

        string shouldProceed = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Do you want to continue?[/]")
                .AddChoices([no, yes]));

        return shouldProceed == yes;
    }
}
