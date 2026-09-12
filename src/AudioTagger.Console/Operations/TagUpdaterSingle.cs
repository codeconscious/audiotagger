using AudioTagger.Library;
using Spectre.Console;

namespace AudioTagger.Console.Operations;

/// <summary>
/// Updates a single supported tag to a specified value for all files in the specific path.
/// </summary>
public sealed class TagUpdaterSingle : IPathOperation
{
    public string Name() => "single-value tag updater";

    private enum TagUpdateType { Overwrite, Prepend, Append, Clear }

    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        printer.Print($"Will update a single tag in {mediaFiles.Count} files:");
        foreach (MediaFile file in mediaFiles)
            printer.Print($"- {file.FileInfo}");

        string tagName = ConfirmUpdateTagName();
        TagUpdateType updateType = ConfirmUpdateType(tagName);
        string tagValue = ConfirmTagValue(tagName, updateType);

        printer.Print($"Will {updateType.ToString().ToUpperInvariant()} the {tagName.ToUpperInvariant()} tag using this text:");
        printer.Print(string.IsNullOrEmpty(tagValue)
            ? "(None)"
            : tagValue, appendLines: 1, fgColor: ConsoleColor.Magenta);

        if (!ConfirmContinue())
        {
            printer.Print("Cancelling!");
            return;
        }

        Watch watch = new();
        uint successCount = 0;
        uint failureCount = 0;

        foreach (MediaFile file in mediaFiles)
        {
            ArgumentNullException.ThrowIfNull(file);

            try
            {
                UpdateTags(file, tagName, tagValue, updateType);
                successCount++;
            }
            catch (FormatException ex)
            {
                failureCount++;
                printer.Error($"{ex.Message} ({file.FileInfo})");
            }
            catch (Exception ex)
            {
                failureCount++;
                printer.Error($"Error for \"{file.FileInfo}\": {ex.Message}");
            }
        }

        string successLabel = successCount == 1 ? "success" : "successes";
        string failureLabel = failureCount == 1 ? "failure" : "failures";
        printer.Print($"Done in {watch.ElapsedFriendly} with {successCount} {successLabel} and {failureCount} {failureLabel}");
    }

    private static string ConfirmTagValue(string tagName, TagUpdateType updateType)
    {
        string updateTypeName = updateType.ToString().ToUpperInvariant();
        return updateType switch
        {
            TagUpdateType.Clear => string.Empty,
            _ => AnsiConsole.Ask<string>($"Enter the text to {updateTypeName} to {tagName.ToUpperInvariant()}: ")
        };
    }

    private static TagUpdateType ConfirmUpdateType(string tagName)
    {
        if (tagName is "year" or "trackNo")
        {
            return TagUpdateType.Overwrite;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<TagUpdateType>()
                .Title($"How do you want to update the {tagName.ToUpperInvariant()} tag?")
                .AddChoices(Enum.GetValues(typeof(TagUpdateType)).Cast<TagUpdateType>()));
    }

    /// <summary>
    /// Asks the user to confirm the tag to update from a given collection.
    /// </summary>
    /// <returns>The internal code-side name of the selected tag for technical operations.</returns>
    private static string ConfirmUpdateTagName()
    {
        // TODO: Refactor with UpdatableFields.cs to DRY things up.
        var dict = new Dictionary<string, string>
        {
            {"Title", "title"},
            {"Album Artists", "albumArtists"},
            {"Artists", "artists"},
            {"Album", "album"},
            {"Genres", "genres"},
            {"Year", "year"},
            {"Comment", "comment"},
            {"Track No.", "trackNo"}
        };

        string response = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which tag do you want to update?")
                .AddChoices(dict.Keys));

        return dict[response];
    }

    private static bool ConfirmContinue()
    {
        const string no = "No!";
        const string yes = "Yes";

        string shouldProceed = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Do you want to continue?[/]")
                .AddChoices(no, yes));

        return shouldProceed == yes;
    }

    private static void UpdateTags(MediaFile mediaFile,
                                   string tagName,
                                   string tagValue,
                                   TagUpdateType updateType)
    {
        switch (tagName)
        {
            case "title":
                string sanitizedTitle = tagValue.Trim().Normalize()
                                             .Replace("___", "　")
                                             .Replace("__", " ");
                mediaFile.Title = GetUpdatedTagValue(mediaFile.Title,
                                                  sanitizedTitle,
                                                  updateType,
                                                  false);
                break;
            case "albumArtists":
                string[] sanitizedAlbumArtists =
                    updateType == TagUpdateType.Clear
                        ? []
                        : tagValue.Replace("___", "　")
                            .Replace("__", " ")
                            .Split(
                                [";"],
                                StringSplitOptions.RemoveEmptyEntries |
                                    StringSplitOptions.TrimEntries)
                            .Select(a => a.Normalize())
                            .ToArray();
                mediaFile.AlbumArtists = GetUpdatedTagValues(mediaFile.AlbumArtists,
                                                          sanitizedAlbumArtists,
                                                          updateType);
                break;
            case "artists":
                string[] sanitizedArtists =
                    updateType == TagUpdateType.Clear
                        ? []
                        : tagValue.Replace("___", "　")
                            .Replace("__", " ")
                            .Split(
                                [";"],
                                StringSplitOptions.RemoveEmptyEntries |
                                    StringSplitOptions.TrimEntries)
                            .Select(a => a.Normalize())
                            .ToArray();
                mediaFile.Artists = GetUpdatedTagValues(mediaFile.Artists,
                                                     sanitizedArtists,
                                                     updateType);
                break;
            case "album":
                string sanitizedAlbum = tagValue.Trim().Normalize()
                                             .Replace("___", "　")
                                             .Replace("__", " ");
                mediaFile.Album = GetUpdatedTagValue(mediaFile.Album,
                                                  sanitizedAlbum,
                                                  updateType,
                                                  false);
                break;
            case "genres":
                string[] sanitizedGenres =
                    updateType == TagUpdateType.Clear
                        ? []
                        : tagValue.Replace("___", "　")
                                  .Replace("__", " ")
                                  .Split(
                                      [";"],
                                      StringSplitOptions.RemoveEmptyEntries |
                                         StringSplitOptions.TrimEntries)
                                  .Select(g => g.Normalize())
                                  .ToArray();
                mediaFile.Genres = GetUpdatedTagValues(mediaFile.Genres,
                                                    sanitizedGenres,
                                                    updateType);
                break;
            case "year":
                mediaFile.Year = ushort.Parse(tagValue);
                break;
            case "trackNo":
                mediaFile.TrackNo = ushort.Parse(tagValue);
                break;
            case "comment":
                mediaFile.Comments = GetUpdatedTagValue(mediaFile.Comments, tagValue, updateType, true);
                break;
            default:
                throw new InvalidOperationException($"Unsupported tag \"{tagName}\" could not be processed.");
        }

        mediaFile.SaveUpdates();

        static string GetUpdatedTagValue(
            string currentValue,
            string newValue,
            TagUpdateType updateType,
            bool useNewLines)
        {
            if (updateType == TagUpdateType.Clear)
            {
                return string.Empty;
            }

            string divider = useNewLines ? Environment.NewLine + Environment.NewLine : string.Empty;
            return updateType switch
            {
                TagUpdateType.Overwrite => newValue,
                TagUpdateType.Prepend   => newValue + divider + currentValue,
                _ =>                       currentValue + divider + newValue
            };
        }

        static string[] GetUpdatedTagValues(
            string[] currentValues,
            string[] newValues,
            TagUpdateType updateType)
        {
            return updateType switch
            {
                TagUpdateType.Clear     => [],
                TagUpdateType.Overwrite => newValues,
                TagUpdateType.Prepend   => [.. newValues, .. currentValues],
                _ =>                       [.. currentValues, .. newValues]
            };
        }
    }
}
