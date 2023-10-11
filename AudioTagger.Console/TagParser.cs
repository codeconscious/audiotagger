using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public class TagParser : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        Regex regex = new("""(?<=[アルバム|シングル][『「]).+(?=[」』])"""); // Make class-level?

        foreach (MediaFile mediaFile in mediaFiles)
        {
            var match = regex.Match(mediaFile.Comments);

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
    }
}
