using Spectre.Console;

namespace AudioTagger;

public class MediaFileViewer
{
    public void PrintFileDetails(MediaFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var table = new Table();
        table.AddColumns("", "");
        table.Border = TableBorder.None;
        table.HideHeaders();
        table.Expand = true;

        table.AddRow("Title", file.Title);
        table.AddRow("Artist", string.Join(", ", file.Artists));
        table.AddRow("Album", file.Album);
        table.AddRow("Year", file.Year.ToString());
        table.AddRow("Duration", file.Duration.ToString("m\\:ss"));

        var genreCount = file.Genres.Length;
        table.AddRow("Genre(s)", string.Join(", ", file.Genres) +
                                 (genreCount > 1 ? $" ({genreCount})" : ""));

        var bitrate = file.BitRate.ToString();
        var sampleRate = file.SampleRate.ToString("#,##0");
        var hasReplayGain = file.HasReplayGainData ? "ReplayGain OK" : "No ReplayGain";
        table.AddRow("Quality", $"{bitrate} kbps | {sampleRate} kHz | {hasReplayGain}");

        if (file.Composers?.Length > 0)
            table.AddRow("Composers", string.Join("; ", file.Composers));

        if (!string.IsNullOrWhiteSpace(file.Comments))
            table.AddRow("Comments", file.Comments);

        table.Columns[0].Width(12);

        var panel = new Panel(table);
        panel.Header("[yellow]" + file.FileNameOnly.Replace("[", "[[").Replace("]", "]]") + "[/]", Justify.Left);
        panel.Border = BoxBorder.Rounded;
        panel.BorderStyle = new Style(Color.Grey19);
        panel.Padding(5, 0, 5, 0);
        panel.Expand = true;

        AnsiConsole.Render(panel);
    }
}