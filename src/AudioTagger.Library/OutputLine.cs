using AudioTagger.Library.MediaFiles;

namespace AudioTagger.Library;

public sealed class OutputLine
{
    public List<LineSubString> Line { get; } = [];

    public OutputLine(params LineSubString[] lineParts)
    {
        Line = lineParts.ToList();
    }

    public OutputLine(string text, ConsoleColor? fgColor = null, ConsoleColor? bgColor = null)
    {
        Line.Add(new LineSubString(text, fgColor, bgColor));
    }

    public static Dictionary<string, string> GetTagKeyValuePairs(
        MediaFile fileData,
        bool includeComments)
    {
        var lines = new Dictionary<string, string>();

        void AddIfPopulated(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                lines.Add(key, value);
        }

        AddIfPopulated("Album Artist(s)", fileData.AlbumArtists.Join());
        AddIfPopulated("Artist(s)", fileData.Artists.Join());
        AddIfPopulated("Title", fileData.Title);
        AddIfPopulated("Album", fileData.Album);
        AddIfPopulated("Year", fileData.Year.ToString());
        AddIfPopulated("Duration", fileData.Duration.ToString("m\\:ss"));

        if (fileData.Genres.Length > 0)
        {
            int genreCount = fileData.Genres.Length;
            lines.Add("Genre(s)", fileData.Genres.Join() +
                                  (genreCount > 1 ? $" ({genreCount})" : ""));
        }

        string bitrate = fileData.BitRate.ToString();
        string sampleRate = fileData.SampleRate.ToString("#,##0");
        const string separator = "  |  ";
        lines.Add(
            "Quality",
            $"{bitrate}kbps" + separator + $"{sampleRate} kHz" + separator + fileData.ReplayGainSummary());

        if (fileData.Composers.Length > 0)
        {
            lines.Add("Composers", string.Join("; ", fileData.Composers));
        }

        if (includeComments)
        {
            AddIfPopulated("Comment", fileData.Comments);
        }

        return lines;
    }
}
