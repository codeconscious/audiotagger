using Spectre.Console;

namespace AudioTagger.Console;

/// <summary>
/// Updates a single supported tag to a specified value for all files in a specific path.
/// </summary>
public sealed class TagUpdaterSingle : IPathOperation
{
    private enum TagUpdateType { Overwrite, Prepend, Append }

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

        printer.Print($"Will update a single tag in {mediaFiles.Count} files:");
        foreach (var file in mediaFiles)
            printer.Print($"- {file.Path}");

        var tagName = ConfirmUpdateTagName();
        var updateType = ConfirmUpdateType(tagName);
        var tagValue = ConfirmTagValue(tagName, updateType);

        printer.Print($"Will {updateType.ToString().ToUpperInvariant()} the {tagName.ToUpperInvariant()} tag using this text:");
        printer.Print(tagValue, appendLines: 1, fgColor: ConsoleColor.Magenta);

        if (!ConfirmContinue())
        {
            printer.Print("Cancelling!");
            return;
        }

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        uint successCount = 0;
        uint failureCount = 0;

        foreach (var file in mediaFiles)
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
                printer.Error($"{ex.Message} ({file.Path})");
            }
            catch (Exception ex)
            {
                failureCount++;
                printer.Error($"Error for \"{file.Path}\": {ex.Message}");
            }
        }

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        var successLabel = successCount == 1 ? "success" : "successes";
        var failureLabel = failureCount == 1 ? "failure" : "failures";
        printer.Print($"Done in {elapsedMs:#,##0}ms with {successCount} {successLabel} and {failureCount} {failureLabel}");
    }

    private static string ConfirmTagValue(string tagName, TagUpdateType updateType)
    {
        var updateTypeName = updateType.ToString().ToUpperInvariant();
        return AnsiConsole.Ask<string>($"Enter the text to {updateTypeName} to {tagName.ToUpperInvariant()}: ");
    }

    private static TagUpdateType ConfirmUpdateType(string tagName)
    {
        if (tagName == "year" || tagName == "trackNo")
            return TagUpdateType.Overwrite;

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

    private static void UpdateTags(MediaFile mediaFile,
                                   string tagName,
                                   string tagValue,
                                   TagUpdateType updateType)
    {
        switch (tagName)
        {
            case "albumArtists":
                var sanitizedAlbumArtists = tagValue.Replace("___", "　")
                                                    .Replace("__", " ")
                                                    .Split(new[] { ";" },
                                                           StringSplitOptions.RemoveEmptyEntries |
                                                           StringSplitOptions.TrimEntries)
                                                    .Select(a => a.Normalize())
                                                    .ToArray();
                mediaFile.AlbumArtists = GetUpdatedValues(mediaFile.AlbumArtists,
                                                          sanitizedAlbumArtists,
                                                          updateType);
                break;
            case "artists":
                var sanitizedArtists = tagValue.Replace("___", "　")
                                               .Replace("__", " ")
                                               .Split(new[] { ";" },
                                                      StringSplitOptions.RemoveEmptyEntries |
                                                      StringSplitOptions.TrimEntries)
                                               .Select(a => a.Normalize())
                                               .ToArray();
                mediaFile.Artists = GetUpdatedValues(mediaFile.Artists,
                                                     sanitizedArtists,
                                                     updateType);
                break;
            case "album":
                var sanitizedAlbum = tagValue.Trim().Normalize()
                                             .Replace("___", "　")
                                             .Replace("__", " ");
                mediaFile.Album = GetUpdatedValue(mediaFile.Album,
                                                  sanitizedAlbum,
                                                  updateType,
                                                  false);
                break;
            case "genres":
                var sanitizedGenres = tagValue.Replace("___", "　")
                                              .Replace("__", " ")
                                              .Split(new[] { ";" },
                                                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                              .Select(g => g.Normalize())
                                              .ToArray();
                mediaFile.Genres = GetUpdatedValues(mediaFile.Genres,
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
                mediaFile.Comments = GetUpdatedValue(mediaFile.Comments, tagValue, updateType, true);
                break;
            default:
                throw new InvalidOperationException($"Unsupported tag \"{tagName}\" could not be processed.");
        }

        mediaFile.SaveUpdates();

        /// <summary>
        /// Returns the new, updated value for a tag.
        /// </summary>
        /// <param name="currentValue">The original value to be modified.</param>
        /// <param name="newValue">The text to be added.</param>
        /// <param name="updateType"></param>
        /// <param name="useNewLine">Whether or not to add line breaks between the new and old text.</param>
        /// <returns></returns>
        static string GetUpdatedValue(string currentValue, string newValue, TagUpdateType updateType, bool useNewLines)
        {
            var divider = useNewLines ? Environment.NewLine + Environment.NewLine : string.Empty;
            return updateType switch
            {
                TagUpdateType.Overwrite => newValue,
                TagUpdateType.Prepend =>   newValue + divider + currentValue,
                _ =>                       currentValue + divider + newValue,
            };
        }

        static string[] GetUpdatedValues(
            string[] currentValues,
            string[] newValues,
            TagUpdateType updateType)
        {
            return updateType switch
            {
                TagUpdateType.Overwrite => newValues,
                TagUpdateType.Prepend =>   newValues.Concat(currentValues).ToArray(),
                _ =>                       currentValues.Concat(newValues).ToArray()
            };
        }
    }
}
