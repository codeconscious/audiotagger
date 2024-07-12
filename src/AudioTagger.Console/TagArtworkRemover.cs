namespace AudioTagger.Console;

public sealed class TagArtworkRemover : IPathOperation
{
    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        var watch = new Watch();
        var failures = 0;

        foreach (MediaFile file in mediaFiles)
        {
            try
            {
                file.RemoveAlbumArt();
                file.SaveUpdates();
            }
            catch (Exception ex)
            {
                printer.Error($"Artwork removal error: {ex.Message}");
                failures++;
            }
        }

        var failureLabel = failures == 1 ? "failure" : "failures";
        printer.Print($"Artwork removed in {watch.ElapsedFriendly} with {failures} {failureLabel}.");
    }
}
