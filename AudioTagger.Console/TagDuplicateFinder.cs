using System.Text;
using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public sealed class TagDuplicateFinder : IPathOperation
{
    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        printer.Print("Checking for duplicates by artist(s) and title...");

        Watch watch = new();

        var exclusions = settings.Duplicates?.Exclusions ?? [];
        printer.Print($"Found {exclusions.Count} exclusion rule(s) in the settings.");

        var includedFiles = exclusions.IsEmpty
            ? mediaFiles
            : mediaFiles.Where(f => !ExcludeFile(f, exclusions)).ToImmutableList();

        if (includedFiles.Count == mediaFiles.Count)
        {
            printer.Print($"No files were excluded via exclusion rules.");
        }
        else
        {
            var diff = mediaFiles.Count - includedFiles.Count;
            var wasWere = diff == 1 ? "was" : "were";
            printer.Print($"Out of {mediaFiles.Count:#,##0} media files, {diff:#,##0} {wasWere} excluded via exclusion rules.");
        }

        var titleReplacements = settings.Duplicates?.TitleReplacements ?? [];
        printer.Print($"Found {titleReplacements.Count} title replacement term(s).");

        var artistReplacements =  new string[] { "The ", "ザ・" }; // TODO: Make a new setting.
        var duplicateGroups = includedFiles
            .ToLookup(m =>
                RemoveSubstrings(m.ArtistSummary, artistReplacements) +
                RemoveSubstrings(m.Title, titleReplacements),
                StringComparer.OrdinalIgnoreCase
            )
            .Where(g => g.Key.HasText() && g.Count() > 1)
            .OrderBy(g => g.Key)
            .ToImmutableArray();

        int groupCount = duplicateGroups.Length;

        if (groupCount == 0)
        {
            printer.Print($"No duplicates were found after {watch.ElapsedFriendly}.");
            return;
        }

        string groupLabel = Utilities.Pluralize(groupCount, "group", "groups");
        printer.Print($"Found {groupCount} duplicate {groupLabel} in {watch.ElapsedFriendly}.");
        PrintResults(duplicateGroups, printer);

        string? searchFor = settings?.Duplicates?.PathSearchFor?.TextOrNull();
        string? replaceWith = settings?.Duplicates?.PathReplaceWith?.TextOrNull();
        CreatePlaylistFile(
            duplicateGroups,
            settings?.Duplicates?.SavePlaylistDirectory,
            (searchFor, replaceWith),
            printer);
    }

    /// <summary>
    /// Determines whether a file should be excluded from duplicate processing due to the exceptions
    /// manually specified in the user's settings file.
    /// </summary>
    /// <returns>Returns `true` if the file should be excluded; otherwise, `false`.</returns>
    private static bool ExcludeFile(MediaFile file, ImmutableList<ExclusionPair> exclusions)
    {
        return exclusions.Any(exclusion =>
        {
            return exclusion switch
            {
                { Artist: string a, Title: string t } =>
                    file.AlbumArtists.Contains(a, StringComparer.OrdinalIgnoreCase) ||
                    file.Artists.Contains(a, StringComparer.OrdinalIgnoreCase) &&
                    file.Title.StartsWith(t, StringComparison.InvariantCultureIgnoreCase),
                { Artist: string a } =>
                    file.AlbumArtists.Contains(a, StringComparer.OrdinalIgnoreCase) ||
                    file.Artists.Contains(a, StringComparer.OrdinalIgnoreCase),
                { Title: string t } =>
                    file.Title.StartsWith(t, StringComparison.InvariantCultureIgnoreCase),
                _ => false
            };
        });
    }

    private static void PrintResults(
        IList<IGrouping<string, MediaFile>> duplicateGroups,
        IPrinter printer)
    {
        int groupIndex = 1;
        int groupIndexPadding = duplicateGroups.Count.ToString().Length + 2;
        int innerIndex = 0;
        const string groupIndexAppend = ". ";

        foreach (IGrouping<string, MediaFile> dupeGroup in duplicateGroups)
        {
            int longestTitleLength = dupeGroup.Max(file => SummarizeArtistTitle(file).Length);

            foreach (MediaFile mediaFile in dupeGroup)
            {
                string header = innerIndex == 0
                    ? groupIndex.ToString().PadLeft(groupIndexPadding) + groupIndexAppend
                    : new string(' ', groupIndexPadding + groupIndexAppend.Length);
                string titleArtist = SummarizeArtistTitle(mediaFile);
                LineSubString titleArtistFormatted = new(header + titleArtist);
                LineSubString separator = new(new string(' ', longestTitleLength - titleArtist.Length + 1));
                LineSubString metadata = new(
                    text: "  " + SummarizeMetadata(mediaFile),
                    fgColor: ConsoleColor.Cyan,
                    bgColor: null,
                    addLineBreak: true
                );
                printer.Print(new LineSubString[] { titleArtistFormatted, separator, metadata });

                innerIndex++;
            }

            groupIndex++;
            innerIndex = 0;
        }

        static string SummarizeArtistTitle(MediaFile mediaFile)
        {
            string artist = string.Join("; ", mediaFile.Artists);
            string title = mediaFile.Title;
            return $"{artist}  /  {title}";
        }

        static string SummarizeMetadata(MediaFile mediaFile)
        {
            string ext = Path.GetExtension(mediaFile.FileInfo.FullName).ToUpperInvariant();
            string bitrate = mediaFile.BitRate + " kpbs";
            string fileSize = mediaFile.FileSizeInBytes.ToString("#,##0") + " bytes";

            double minutes = Math.Floor(mediaFile.Duration.TotalSeconds / 60);
            double seconds = Math.Ceiling(mediaFile.Duration.TotalSeconds % 60);
            string time = $"{minutes}:{seconds:00}";

            return $"({ext[1..]}; {bitrate}; {time}; {fileSize})";
        }
    }

    /// <summary>
    /// Removes occurrences of each of a collection of substrings from a string.
    /// Returns the source string as-is if no replacement terms were passed in.
    /// </summary>
    private static string RemoveSubstrings(string source, ICollection<string> terms)
    {
        return terms switch
        {
            null or { Count: 0 } => source,
            _ => terms.Aggregate(
                    new StringBuilder(source),
                    (sb, term) => sb.Replace(term, string.Empty),
                    sb => sb.ToString())
        };
    }

    /// <summary>
    /// Creates a playlist list in M3U playlist format.
    /// </summary>
    /// <param name="duplicateGroups"></param>
    /// <param name="replacements">Optionally replace parts of the file paths.</param>
    /// <param name="printer"></param>
    private static void CreatePlaylistFile(
        ImmutableArray<IGrouping<string, MediaFile>> duplicateGroups,
        string? saveDirectory,
        (string? SearchFor, string? ReplaceWith) replacements,
        IPrinter printer)
    {
        if (saveDirectory is null)
        {
            string appDir = AppContext.BaseDirectory;
            printer.Warning($"No playlist save directory was specified, so saving to the application directory at \"{appDir}\".");
            saveDirectory = appDir;
        }
        else if (!Path.Exists(saveDirectory))
        {
            printer.Error($"Cannot save playlist because \"{saveDirectory}\" doesn't exist.");
            return;
        }

        StringBuilder contents = new("#EXTM3U\n");
        string now = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"Duplicates by AudioTagger - {now}.m3u";
        string fullPath = Path.Combine(saveDirectory, filename);

        duplicateGroups
            .SelectMany(g => g)
            .ToList()
            .ForEach(m =>
            {
                double seconds = m.Duration.TotalSeconds;
                string artistTitle = $"{string.Join(", ", m.ArtistSummary)} - {m.Title}";
                string extInf = $"#EXTINF:{seconds},{artistTitle}";
                contents.AppendLine(extInf);

                string updatedPath = replacements.SearchFor is null ||
                                     replacements.ReplaceWith is null
                    ? m.FileInfo.FullName
                    : m.FileInfo.FullName.Replace(replacements.SearchFor, replacements.ReplaceWith);

                contents.AppendLine(updatedPath);
            });

        try
        {
            File.WriteAllText(fullPath, contents.ToString());
            printer.Print($"Saved playlist file to \"{filename}\".");
        }
        catch (Exception ex)
        {
            printer.Error($"Couldn't write to playlist file at \"{fullPath}\": {ex.Message}");
        }
    }
}
