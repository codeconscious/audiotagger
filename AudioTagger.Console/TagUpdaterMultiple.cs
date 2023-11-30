using Spectre.Console;

namespace AudioTagger.Console;

/// <summary>
/// Updates a single supported tag to a specified value for all files in a specific path.
/// </summary>
public sealed class TagUpdaterMultiple : IPathOperation
{
    private static readonly string _inputFile = "input.txt";

    private enum TagUpdateType { Overwrite, Prepend, Append }

    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        List<MediaFile> sortedMediaFiles = mediaFiles.OrderBy(f => $"{f.TrackNo:00000}{f.Title}")
                                                     .ToList();

        printer.Print($"Will update the title tag of {sortedMediaFiles.Count} file(s):");
        sortedMediaFiles.ForEach(f => printer.Print($"- {f.Path}"));

        string[] inputLines;
        try
        {
            inputLines = File.ReadAllLines(_inputFile);
        }
        catch (FileNotFoundException)
        {
            printer.Error($"Could not find the file {_inputFile}");
            return;
        }
        catch (Exception ex)
        {
            printer.Error($"Couldn't read input file: {ex.Message}");
            return;
        }

        if (inputLines.Length != sortedMediaFiles.Count)
        {
            printer.Error($"Cannot match the {inputLines.Length} input lines to the {sortedMediaFiles.Count} media files.");
            return;
        }

        string tagName = ConfirmUpdateTagName();
        TagUpdateType updateType = ConfirmUpdateType(tagName);

        Table table = new();
        table.AddColumns("Filename", "Current", "Proposed new");
        for (int i = 0; i < inputLines.Length; i++)
        {
            MediaFile thisFile = sortedMediaFiles[i];
            table.AddRow(
                Markup.Escape(thisFile.FileNameOnly),
                GetTagValue(thisFile, tagName),
                inputLines[i]);
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

        var updateSet = sortedMediaFiles.Zip(inputLines, (f, l) => (File: f, NewTitle: l));

        foreach ((MediaFile File, string NewTitle) pair in updateSet)
        {
            ArgumentNullException.ThrowIfNull(pair);

            try
            {
                UpdateTags(pair.File, tagName, pair.NewTitle, updateType);
                successCount++;
            }
            catch (FormatException ex)
            {
                failureCount++;
                printer.Error($"{ex.Message} ({pair.File.Path})");
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
        printer.Print($"Done in {elapsedMs:#,##0}ms with {successCount} {successLabel} and {failureCount} {failureLabel}");
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
            {"Title", "title"},
            {"Album Artists", "albumArtists"},
            {"Artists", "artists"},
            {"Album", "album"},
            {"Genres", "genres"},
            {"Year", "year"},
            {"Comment", "comment"},
            {"Track No.", "trackNo"},
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
                .AddChoices([no, yes]));

        return shouldProceed == yes;
    }

    private static string GetTagValue(MediaFile mediaFile, string tagName) =>
        Markup.Escape(
            tagName switch
            {
                "title"        => mediaFile.Title,
                "albumArtists" => string.Join("; ", mediaFile.AlbumArtists),
                "artists"      => string.Join("; ", mediaFile.Artists),
                "album"        => mediaFile.Album,
                "genres"       => string.Join("; ", mediaFile.Genres),
                "year"         => mediaFile.Year.ToString(),
                "comment"      => mediaFile.Comments,
                "trackNo"      => mediaFile.TrackNo.ToString(),
                _              => throw new ArgumentException($"\"{tagName}\" is an invalid tagName.")
            });

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
                mediaFile.Title = GetUpdatedValue(mediaFile.Title,
                                                  sanitizedTitle,
                                                  updateType,
                                                  false);
                break;
            case "albumArtists":
                string[] sanitizedAlbumArtists = tagValue.Replace("___", "　")
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
                string[] sanitizedArtists = tagValue.Replace("___", "　")
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
                string sanitizedAlbum = tagValue.Trim().Normalize()
                                             .Replace("___", "　")
                                             .Replace("__", " ");
                mediaFile.Album = GetUpdatedValue(mediaFile.Album,
                                                  sanitizedAlbum,
                                                  updateType,
                                                  false);
                break;
            case "genres":
                string[] sanitizedGenres = tagValue.Replace("___", "　")
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
            string divider = useNewLines ? Environment.NewLine + Environment.NewLine : string.Empty;
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
