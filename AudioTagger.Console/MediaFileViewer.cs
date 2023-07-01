using Spectre.Console;

namespace AudioTagger;

public class MediaFileViewer
{
    public void PrintFileDetails(MediaFile file)
    {
        // TODO: Handle colors more gracefully.
        var tagNameFormatter = (string s) => "[grey]" + s +"[/]";

        var table = new Table();
        table.AddColumns("", "");
        table.Border = TableBorder.None;
        table.HideHeaders();
        table.Expand = true;

        table.AddRow(tagNameFormatter("Title"), file.Title.EscapeMarkup());
        if (file.AlbumArtists.Any())
        {
            table.AddRow(tagNameFormatter("Album Artist"),
                         file.AlbumArtistsCombined.EscapeMarkup());
        }
        table.AddRow(tagNameFormatter("Artist"), file.ArtistsCombined.EscapeMarkup());
        table.AddRow(tagNameFormatter("Album"), file.Album.EscapeMarkup());
        if (file.TrackNo > 0)
            table.AddRow(tagNameFormatter("Track"), file.TrackNo.ToString());
        if (file.Year > 0)
            table.AddRow(tagNameFormatter("Year"), file.Year.ToString());
        table.AddRow(tagNameFormatter("Duration"), file.Duration.ToString("m\\:ss"));

        var genreCount = file.Genres.Length;
        table.AddRow(tagNameFormatter("Genres"),
                     file.GenresCombined.EscapeMarkup() +
                        (genreCount > 1 ? $" ({genreCount})" : ""));

        var bitrate = file.BitRate.ToString();
        var sampleRate = file.SampleRate.ToString("#,##0");

        table.AddRow(tagNameFormatter("Quality"), $"{bitrate} kbps @ {sampleRate} kHz | {file.ReplayGainSummary()}");

        if (file.Composers?.Length > 0)
        {
            table.AddRow(
                tagNameFormatter("Composers"),
                file.ComposersCombined.EscapeMarkup());
        }

        if (!string.IsNullOrWhiteSpace(file.Comments))
            table.AddRow(tagNameFormatter("Comments"), file.Comments.EscapeMarkup());

        if (!string.IsNullOrWhiteSpace(file.Description))
            table.AddRow(tagNameFormatter("Comments"), file.Description.EscapeMarkup());

        if (!string.IsNullOrWhiteSpace(file.Lyrics))
            table.AddRow(tagNameFormatter("Lyrics"), file.Lyrics[..25].EscapeMarkup() + "...");

        table.Columns[0].Width(15);

        var panel = new Panel(table);
        panel.Header("[yellow]" + file.FileNameOnly.EscapeMarkup() + "[/]", Justify.Left);
        panel.Border = BoxBorder.Rounded;
        panel.BorderStyle = new Style(Color.Grey15);
        panel.Padding(5, 0, 5, 0);
        panel.Expand = true;

        AnsiConsole.Write(panel);
    }

    public TableRow PrintFileSummary(MediaFile file)
    {
        var rows = new List<string>
        {
            file.AlbumArtistsAndArtistsCombined.EscapeMarkup(),
            file.Album.EscapeMarkup(),
            file.TrackNo == 0 ? string.Empty : file.TrackNo.ToString().EscapeMarkup(),
            file.Title.EscapeMarkup(),
            file.Year == 0 ? string.Empty : file.Year.ToString(),
            file.GenresCombined.EscapeMarkup(),
            file.Duration.ToString("m\\:ss"),
            file.ReplayGainTrack.ToString()
        };

        var markups = rows.Select(r => new Markup(r));

        return new TableRow(markups);
    }
}
