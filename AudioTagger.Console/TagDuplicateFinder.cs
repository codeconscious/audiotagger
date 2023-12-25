using System.Text.RegularExpressions;
using System.Text;

namespace AudioTagger.Console;

public sealed class TagDuplicateFinder : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        ImmutableList<string> titleReplacements = settings?.Duplicates?.TitleReplacements ?? [];
        printer.Print($"Found {titleReplacements.Count} replacement term(s).");
        printer.Print("Checking for duplicates by artist(s) and title...");

        Timer timer = new();

        var duplicateGroups = mediaFiles
            .ToLookup(m => ConcatenateCollectionText(m.Artists) +
                           RemoveSubstrings(m.Title, titleReplacements))
            .Where(m => !string.IsNullOrWhiteSpace(m.Key) && m.Count() > 1)
            .OrderBy(m => m.Key)
            .ToImmutableArray();

        int count = duplicateGroups.Length;

        printer.Print($"Found {count} duplicate group{(count == 1 ? string.Empty : "s")} in {timer.ElapsedTimeFriendly()}.");
        PrintResults(duplicateGroups, printer);
    }

    private static void PrintResults(IList<IGrouping<string, MediaFile>> duplicateGroups, IPrinter printer)
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
            string ext = Path.GetExtension(mediaFile.Path).ToUpperInvariant();
            string bitrate = mediaFile.BitRate + " kpbs";
            string fileSize = mediaFile.FileSizeInBytes.ToString("#,##0") + " bytes";

            double minutes = Math.Floor(mediaFile.Duration.TotalSeconds / 60);
            double seconds = Math.Ceiling(mediaFile.Duration.TotalSeconds % 60);
            string time = $"{minutes}:{seconds:00}";

            return $"({ext[1..]}; {bitrate}; {time}; {fileSize})";
        }
    }

    /// <summary>
    /// Removes parts of a string that should be ignored for comparison.
    /// For example, "The Beatles" would convert to "Beatles" because "The"
    /// should not be included in the comparison.
    /// </summary>
    /// <param name="strings"></param>
    private static string ConcatenateCollectionText(IEnumerable<string> strings)
    {
        var concatenated = string.Concat(strings).ToLowerInvariant().Trim();
        return Regex.Replace(concatenated, "^the", string.Empty);
    }

    /// <summary>
    /// Removes occurrences of each of a collection of substrings from a string.
    /// Returns the source string as-is if no replacement terms were passed in.
    /// </summary>
    private static string RemoveSubstrings(string source, ImmutableList<string> terms)
    {
        return terms switch
        {
            null or { Count: 0 } => source,
            _                    => terms.ToList()
                                         .Aggregate(
                                             new StringBuilder(source),
                                             (sb, term) => sb.Replace(term, string.Empty),
                                             sb => sb.ToString())
        };
    }
}
