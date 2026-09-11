using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

// This has been replaced with my AudioTagTool application instead.
// It will likely no longer be updated.
public sealed class TagCacher : IPathOperation
{
    public string Name() => "tag cacher";

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
            mediaFiles.Select(m => new
            {
                m.FileNameOnly,
                m.FileInfo.DirectoryName,
                m.Artists,
                m.AlbumArtists,
                m.Album,
                m.TrackNo,
                m.Title,
                m.Year,
                m.Genres,
                m.Duration,
                m.FileInfo.LastWriteTime,
            });

        printer.Print("Serializing tag data to JSON...");
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        var json = JsonSerializer.Serialize(summaries, options);

        printer.Print($"Saving cached tag data to \"{settings.TagCacheFilePath}\"...");
        File.WriteAllText(settings.TagCacheFilePath, json);
        printer.Print($"Saved in {watch.ElapsedFriendly}.");
    }
}
