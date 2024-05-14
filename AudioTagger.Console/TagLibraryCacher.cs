using System.Text.Json;

namespace AudioTagger.Console;

public sealed class TagLibraryCacher : IPathOperation
{
    private record TagSummer(
        string[] Artists,
        string Album,
        uint TrackNo,
        string Title,
        uint Year,
        string[] Genres,
        TimeSpan Duration
        // string Comment // TODO: 追加するかどうか決めよう。もしそうならば、特別な処理が必要か検討を。
    );

    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        if (settings.TagLibraryFilePath is null)
        {
            printer.Error("You must specify the save file path.");
            return;
        }

        Watch watch = new();

        var tagSummary = mediaFiles.Select(m => {
                return new TagSummer(
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
        string json = JsonSerializer.Serialize(tagSummary, options);

        printer.Print($"Saving cached tag data to \"{settings.TagLibraryFilePath}\"...");
        File.WriteAllText(settings.TagLibraryFilePath, json);
        printer.Print($"Saved in {watch.ElapsedFriendly}.");
    }
}
