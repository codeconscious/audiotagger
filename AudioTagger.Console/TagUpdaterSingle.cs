using Spectre.Console;

namespace AudioTagger.Console;

/// <summary>
/// Updates a single supported tag to a specified value for all files in a specific path.
/// </summary>
public class TagUpdaterSingle : IPathOperation
{
    public TagUpdaterSingle() { }

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

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        printer.Print($"Will update a single tag in {mediaFiles.Count} files:");
        foreach (var file in mediaFiles)
            printer.Print($"- {file.Path}");

        var tagName = GetTagName();
        var tagValue = GetTagValue(tagName);

        printer.Print($"Updating the {tagName.ToUpperInvariant()} tag to the following value:");
        printer.Print(tagValue, fgColor: ConsoleColor.Magenta);

        if (!ConfirmContinue())
        {
            printer.Print("Cancelling!");
            return;
        }

        uint successCount = 0;
        uint failureCount = 0;

        foreach (var file in mediaFiles)
        {
            ArgumentNullException.ThrowIfNull(file);

            try
            {
                UpdateTags(file, tagName, tagValue);
                successCount++;
            }
            catch
            {
                failureCount++;
                printer.Print($"✖️ ERROR: {file.Path}");
            }
        }

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        printer.Print($"Done in {elapsedMs:#,##0}ms -- {successCount} successes, {failureCount} failures");
    }

    private static string GetTagName()
    {
        // TODO: Refactor with UpdatableFields.cs to DRY things up.
        var dict = new Dictionary<string, string>
        {
            {"Album Artists", "albumArtists"},
            {"Artists", "artists"},
            {"Album", "album"},
            {"Genres", "genres"},
            {"Year", "year"},
            {"Comment", "comment"},
            {"Track No.", "trackNo"},
        };

        var response = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which tag do you want to update?")
                    .AddChoices(dict.Keys));

        return dict[response];
    }

    private static string GetTagValue(string tagName)
    {
        return AnsiConsole.Ask<string>($"Enter the new value for {tagName.ToUpperInvariant()}: ");
    }

    private static bool ConfirmContinue()
    {
        const string no = "No!";
        const string yes = "Yes";

        var shouldProceed = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Do you want to continue?[/]")
                .AddChoices(new[] { no, yes }));

        return shouldProceed == yes;
    }

    private static void UpdateTags(MediaFile mediaFile, string tagName, string tagValue)
    {
        switch (tagName)
        {
            case "albumArtists":
                mediaFile.AlbumArtists = tagValue.Replace("___", "　")
                                                 .Replace("__", " ")
                                                 .Split(new[] { ";" },
                                                         StringSplitOptions.RemoveEmptyEntries |
                                                         StringSplitOptions.TrimEntries)
                                                 .Select(a => a.Normalize())
                                                 .ToArray();
                break;
            case "artists":
                mediaFile.Artists = tagValue.Replace("___", "　")
                                            .Replace("__", " ")
                                            .Split(new[] { ";" },
                                                   StringSplitOptions.RemoveEmptyEntries |
                                                   StringSplitOptions.TrimEntries)
                                            .Select(a => a.Normalize())
                                            .ToArray();
                break;
            case "album":
                mediaFile.Album = tagValue.Trim().Normalize()
                                          .Replace("___", "　")
                                          .Replace("__", " ");
                break;
            case "genres":
                mediaFile.Genres = tagValue.Replace("___", "　")
                                           .Replace("__", " ")
                                           .Split(new[] { ";" },
                                                  StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Select(g => g.Normalize())
                                           .ToArray();
                break;
            case "year":
                mediaFile.Year = ushort.Parse(tagValue);
                break;
            case "trackNo":
                mediaFile.TrackNo = ushort.Parse(tagValue);
                break;
            case "comment":
                mediaFile.Comments = tagValue;
                break;
            default:
                throw new InvalidOperationException($"Unsupported tag \"{tagName}\" could not be processed.");
        }

        mediaFile.SaveUpdates();
    }
}
