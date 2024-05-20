using System.Text.Json;

namespace AudioTagger.Console;

public sealed class TagLibraryCacher : IPathOperation
{
    private record TagSummary(
        string[] Artists,
        string Album,
        uint TrackNo,
        string Title,
        uint Year,
        string[] Genres,
        TimeSpan Duration
    );

    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        if (settings.TagLibraryFilePath is null)
        {
            printer.Error("You must specify the save file path in the settings file.");
            return;
        }

        Watch watch = new();

        var tagDtos = mediaFiles.Select(m => {
                return new TagSummary(
                    m.Artists,
                    m.Album,
                    m.TrackNo,
                    m.Title,
                    m.Year,
                    m.Genres,
                    m.Duration
                );
            });

        printer.Print("Serializing the tags to JSON...");
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(
                        System.Text.Unicode.UnicodeRanges.All)
        };
        var json = JsonSerializer.Serialize(tagDtos, options);
        var unescapedJson = System.Text.RegularExpressions.Regex.Unescape(json); // Avoids `\0027`, etc.

        printer.Print($"Saving cached tag data to \"{settings.TagLibraryFilePath}\"...");
        File.WriteAllText(settings.TagLibraryFilePath, unescapedJson);
        printer.Print($"Saved in {watch.ElapsedFriendly}.");
    }
}
