using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public sealed class TagParser : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        Timer timer = new();

        Regex regex = new("""(?<=[アルバム|シングル][『「]).+(?=[」』])"""); // Make class-level?

        foreach (MediaFile mediaFile in mediaFiles)
        {
            Match match = regex.Match(mediaFile.Comments);

            if (!match.Success || mediaFile.Album == match.Value)
            {
                printer.Print($"No changes needed for \"{mediaFile.FileNameOnly}\".");
                continue;
            }

            try
            {
                mediaFile.SaveUpdates();
                printer.Print($"Wrote album \"{match.Value}\" to file \"{mediaFile.FileNameOnly}\"...");
            }
            catch (Exception ex)
            {
                printer.Error($"Error writing album to \"{mediaFile.FileNameOnly}\": {ex.Message}");
            }
        }

        printer.Print($"Done in {timer.ElapsedTimeFriendly()}.");
    }
}
