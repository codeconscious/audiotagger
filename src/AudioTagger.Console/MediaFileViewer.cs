using System.Globalization;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class MediaFileViewer
{
    public void PrintFileDetails(MediaFile file)
    {
        // TODO: Handle colors more gracefully.
        string TagNameFormatter(string s) => "[grey]" + s + "[/]";

        Table table = new();
        table.AddColumns(string.Empty, string.Empty);
        table.Border = TableBorder.None;
        table.HideHeaders();
        table.Expand = true;

        table.AddRow(TagNameFormatter("Title"), file.Title.EscapeMarkup());
        if (file.AlbumArtists.Any())
        {
            table.AddRow(TagNameFormatter("Album Artist"),
                         file.AlbumArtists.Join().EscapeMarkup());
        }
        table.AddRow(TagNameFormatter("Artist"), file.Artists.Join().EscapeMarkup());
        table.AddRow(TagNameFormatter("Album"), file.Album.EscapeMarkup());
        if (file.TrackNo > 0)
            table.AddRow(TagNameFormatter("Track"), file.TrackNo.ToString());
        if (file.Year > 0)
            table.AddRow(TagNameFormatter("Year"), file.Year.ToString());
        table.AddRow(TagNameFormatter("Duration"), file.Duration.ToString("m\\:ss"));

        int genreCount = file.Genres.Length;
        table.AddRow(TagNameFormatter("Genres"),
                     file.Genres.Join().EscapeMarkup() +
                        (genreCount > 1 ? $" ({genreCount})" : string.Empty));

        string bitrate = file.BitRate.ToString();
        string sampleRate = file.SampleRate.ToString("#,##0");

        table.AddRow(TagNameFormatter("Quality"), $"{bitrate} kbps @ {sampleRate} kHz | {file.ReplayGainSummary()}");

        if (file.Composers.Length > 0)
        {
            table.AddRow(
                TagNameFormatter("Composers"),
                file.Composers.Join().EscapeMarkup());
        }

        if (file.Comments.HasText())
            table.AddRow(TagNameFormatter("Comments"), file.Comments.EscapeMarkup());

        if (file.Description.HasText())
            table.AddRow(TagNameFormatter("Comments"), file.Description.EscapeMarkup());

        if (file.Lyrics.HasText())
            table.AddRow(TagNameFormatter("Lyrics"), file.Lyrics[..25].EscapeMarkup() + "...");

        table.Columns[0].Width(15);

        Panel panel = new(table);
        panel.Header("[yellow]" + file.FileNameOnly.EscapeMarkup() + "[/]", Justify.Left);
        panel.Border = BoxBorder.Rounded;
        panel.BorderStyle = new Style(Color.Grey15);
        panel.Padding(5, 0, 5, 0);
        panel.Expand = true;

        AnsiConsole.Write(panel);
    }

    public static TableRow PrintFileSummary(MediaFile file)
    {
        var rows = new List<string>
        {
            file.ArtistSummary.EscapeMarkup(),
            file.Album.EscapeMarkup(),
            file.TrackNo == 0 ? string.Empty : file.TrackNo.ToString().EscapeMarkup(),
            file.Title.EscapeMarkup(),
            file.Year == 0 ? string.Empty : file.Year.ToString(),
            file.Genres.Join().EscapeMarkup(),
            file.Duration.ToString("m\\:ss"),
            file.ReplayGainTrack.ToString(CultureInfo.InvariantCulture)
        };

        IEnumerable<Markup> markups = rows.Select(r => new Markup(r));

        return new TableRow(markups);
    }
}
