using System.Text.RegularExpressions;
using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

public sealed class TagParser : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        Watch watch = new();

        Regex regex = new("(?<=[アルバム|シングル][『「]).+(?=[」』])"); // Make class-level?

        foreach (MediaFile mediaFile in mediaFiles)
        {
            // The media field can be customized as needed.
            Match match = regex.Match(mediaFile.Comments);

            if (!match.Success || mediaFile.Album == match.Value)
            {
                printer.Print($"No changes needed for \"{mediaFile.FileNameOnly}\".");
                continue;
            }

            mediaFile.Album = match.Value;

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

        printer.Print($"Done in {watch.ElapsedFriendly}.");
    }
}
