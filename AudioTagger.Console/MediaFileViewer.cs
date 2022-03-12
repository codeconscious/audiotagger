using Spectre.Console;

namespace AudioTagger;

public class MediaFileViewer
{
    public void PrintFileDetails(MediaFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        // TODO: Handle colors more gracefully.
        var tagNameFormatter = (string s) => "[grey]" + s +"[/]";

        var table = new Table();
        table.AddColumns("", "");
        table.Border = TableBorder.None;
        table.HideHeaders();
        table.Expand = true;

        table.AddRow(tagNameFormatter("Title"), file.Title);
        table.AddRow(tagNameFormatter("Artist"), string.Join(", ", file.Artists));
        table.AddRow(tagNameFormatter("Album"), file.Album);
        table.AddRow(tagNameFormatter("Track"), file.TrackNo.ToString());
        table.AddRow(tagNameFormatter("Year"), file.Year.ToString());
        table.AddRow(tagNameFormatter("Duration"), file.Duration.ToString("m\\:ss"));

        var genreCount = file.Genres.Length;
        table.AddRow(tagNameFormatter("Genres"), string.Join(", ", file.Genres) +
                                 (genreCount > 1 ? $" ({genreCount})" : ""));

        var bitrate = file.BitRate.ToString();
        var sampleRate = file.SampleRate.ToString("#,##0");
        var hasReplayGain = file.HasReplayGainData ? "ReplayGain OK" : "No ReplayGain";
        table.AddRow(tagNameFormatter("Quality"), $"{bitrate} kbps @ {sampleRate} kHz | {hasReplayGain}");

        if (file.Composers?.Length > 0)
        {
            table.AddRow(
                tagNameFormatter("Composers"),
                string.Join("; ", file.Composers));
        }

        if (!string.IsNullOrWhiteSpace(file.Comments))
            table.AddRow(tagNameFormatter("Comments"), file.Comments);

        if (!string.IsNullOrWhiteSpace(file.Lyrics))
            table.AddRow(tagNameFormatter("Lyrics"), file.Lyrics[..25] + "...");

        table.Columns[0].Width(5);

        var panel = new Panel(table);
        panel.Header("[yellow]" + Utilities.SanitizeSpectreString(file.FileNameOnly) + "[/]", Justify.Left);
        panel.Border = BoxBorder.Rounded;
        panel.BorderStyle = new Style(Color.Grey15);
        panel.Padding(5, 0, 5, 0);
        panel.Expand = true;

        AnsiConsole.Write(panel);
    }

    public TableRow PrintFileSummary(MediaFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var rows = new List<string>
        {
            string.Join(", ", file.Artists),
            file.Album,
            file.TrackNo == 0 ? string.Empty : file.TrackNo.ToString(),
            file.Title,
            file.Year == 0 ? string.Empty : file.Year.ToString(),
            file.Duration.ToString("m\\:ss")
        };

        var markups = rows.Select(r => new Markup(r));

        return new TableRow(markups);
    }
}