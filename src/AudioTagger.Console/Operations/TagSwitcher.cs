using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

public sealed class TagSwitcher : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        var watch = new Watch();

        printer.Print($"Found {mediaFiles.Count} audio files.");

        foreach (var file in mediaFiles)
        {
            // These fields are intentionally switched like this!
            var artist = file.Title;
            var title = file.Artists.First();

            file.Title = title;
            file.Artists[0] = artist;

            try
            {
                file.SaveUpdates();
                printer.Print($"Saved \"{file.FileNameOnly}\".");
            }
            catch (Exception e)
            {
                printer.Error($"Error saving \"{file.FileNameOnly}\": {e}");
            }
        }

        printer.Print($"Done in {watch.ElapsedFriendly}.");
    }
}
