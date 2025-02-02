using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

public sealed class TagCacher : IPathOperation
{
    private record TagSummary(
        string[] Artists,
        string[] AlbumArtists,
        string Album,
        uint TrackNo,
        string Title,
        uint Year,
        string[] Genres,
        TimeSpan Duration,
        DateTime UpdatedAt
    );

    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        if (settings.TagCacheFilePath is null)
        {
            printer.Error("You must specify the save file path in the settings.");
            return;
        }

        Watch watch = new();

        var summaries =
            mediaFiles.Select(m => new TagSummary(
                m.Artists,
                m.AlbumArtists,
                m.Album,
                m.TrackNo,
                m.Title,
                m.Year,
                m.Genres,
                m.Duration,
                m.FileInfo.LastWriteTime
            ));

        printer.Print("Serializing the tags to JSON...");
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        var json = JsonSerializer.Serialize(summaries, options);
        var unescapedJson = System.Text.RegularExpressions.Regex.Unescape(json); // Avoids `\0027`, etc.

        printer.Print($"Saving cached tag data to \"{settings.TagCacheFilePath}\"...");
        File.WriteAllText(settings.TagCacheFilePath, unescapedJson);
        printer.Print($"Saved in {watch.ElapsedFriendly}.");
    }
}
