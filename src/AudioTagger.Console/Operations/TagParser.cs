using System.Text.RegularExpressions;
using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

public sealed class TagParser : IPathOperation
{
    public string Name() => "tag parser";

    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        Watch watch = new();

        // This needs to be manually updated each time, as does the field to update.
        // It's not ideal, but I only need to do this very rarely.
        Regex regex = new("(?<=\\d - )[^「]+(?= - .+\\..+)");

        foreach (MediaFile file in mediaFiles)
        {
            // The media field can be customized as needed.
            Match match = regex.Match(file.FileNameOnly);

            if (!match.Success || file.Artists[0] == match.Value)
            {
                printer.Print($"No changes needed for \"{file.FileNameOnly}\".");
                continue;
            }

            file.Artists = [match.Value];

            try
            {
                file.SaveUpdates();
                printer.Print($"Wrote artist \"{match.Value}\" to file \"{file.FileNameOnly}\"...");
            }
            catch (Exception ex)
            {
                printer.Error($"Error writing artist to \"{file.FileNameOnly}\": {ex.Message}");
            }
        }

        printer.Print($"Done in {watch.ElapsedFriendly}.");
    }
}
