using AudioTagger.Library.MediaFiles;

namespace AudioTagger;

public sealed class OutputLine
{
    public List<LineSubString> Line { get; set; } = [];

    public OutputLine() { }

    public OutputLine(LineSubString lineParts)
    {
        Line = [lineParts];
    }

    public OutputLine(params LineSubString[] lineParts)
    {
        Line = lineParts.ToList();
    }

    public OutputLine(string text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
    {
        Line.Add(new LineSubString(text, fgColor, bgColor));
    }

    public void Add(LineSubString lineParts)
    {
        Line.Add(lineParts);
    }

    public void Add(string text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
    {
        Line.Add(new LineSubString(text, fgColor, bgColor));
    }

    public static OutputLine TagDataWithHeader(string tagName,
                                               IReadOnlyList<LineSubString> tagData,
                                               string prependLine = "")
    {
        const int spacesToPrepend = 4;
        int spacesToAppend = 13 - tagName.Length; // TODO: Calculate this instead

        var lineOutput = new OutputLine();

        lineOutput.Add(prependLine);
        lineOutput.Add(new string(' ', spacesToPrepend));
        lineOutput.Add(tagName);
        lineOutput.Add(new string(' ', spacesToAppend));

        foreach (LineSubString part in tagData)
            lineOutput.Add(part);

        return lineOutput;
    }

    public static OutputLine TagDataWithHeader(string tagName, string tagData,
                                               string prependLine = "")
    {
        return TagDataWithHeader(
            tagName,
            new List<LineSubString> { new(tagData) },
            prependLine);
    }

    public static IList<OutputLine> GetTagPrintedLines(MediaFile fileData)
    {
        var lines = new List<OutputLine>
        {
            TagDataWithHeader("Title", fileData.Title),
            TagDataWithHeader("Artist(s)", fileData.Artists.Join()),
            TagDataWithHeader("Album", fileData.Album),
            TagDataWithHeader("Year", fileData.Year.ToString()),
            TagDataWithHeader("Duration", fileData.Duration.ToString("m\\:ss"))
        };

        int genreCount = fileData.Genres.Length;
        lines.Add(TagDataWithHeader("Genre(s)", string.Join(", ", fileData.Genres) +
                                                (genreCount > 1 ? $" ({genreCount})" : string.Empty)));

        string bitrate = fileData.BitRate.ToString();
        string sampleRate = fileData.SampleRate.ToString("#,##0");

        // Create formatted quality line
        const string genreSeparator = "    ";
        lines.Add(
            TagDataWithHeader(
                "Quality",
                new List<LineSubString>
                {
                    new(bitrate),
                    new(" kbps" + genreSeparator, ConsoleColor.DarkGray),
                    new(sampleRate),
                    new(" kHz" + genreSeparator, ConsoleColor.DarkGray),
                    new(fileData.ReplayGainSummary())
                }));

        if (fileData.Composers?.Length > 0)
            lines.Add(TagDataWithHeader("Composers", fileData.Composers.Join()));

        if (!string.IsNullOrWhiteSpace(fileData.Comments))
            lines.Add(TagDataWithHeader("Comment", fileData.Comments));

        return lines;
    }

    public static Dictionary<string, string> GetTagKeyValuePairs(
        MediaFile fileData,
        bool includeComments)
    {
        var lines = new Dictionary<string, string>
        {
            { "Title", fileData.Title },
            { "Artist(s)", fileData.Artists.Join() },
            { "Album", fileData.Album },
            { "Year", fileData.Year.ToString() },
            { "Duration", fileData.Duration.ToString("m\\:ss") }
        };

        int genreCount = fileData.Genres.Length;
        lines.Add("Genre(s)", fileData.Genres.Join() +
                              (genreCount > 1 ? $" ({genreCount})" : ""));

        string bitrate = fileData.BitRate.ToString();
        string sampleRate = fileData.SampleRate.ToString("#,##0");

        // Create formatted quality line
        const string separator = "  |  ";
        lines.Add(
            "Quality",
            $"{bitrate}kbps" + separator + $"{sampleRate} kHz" + separator + fileData.ReplayGainSummary());

        if (fileData.Composers?.Length > 0)
            lines.Add("Composers", string.Join("; ", fileData.Composers));

        if (includeComments && !string.IsNullOrWhiteSpace(fileData.Comments))
            lines.Add("Comment", fileData.Comments);

        return lines;
    }
}
