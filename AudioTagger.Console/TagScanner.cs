using System.Text;
using System.Text.RegularExpressions;

namespace AudioTagger.Console;

public sealed class TagScanner : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        var mp3s = mediaFiles.Where(f => Path.GetExtension(f.FileNameOnly) == ".mp3");

        if (!mp3s.Any())
        {
            printer.Print("There are no MP3 files to work on. Cancelling...");
            return;
        }

        printer.Print($"Found {mp3s.Count()} MP3 files.");

        var urlRegex = new Regex("""(?<=URL: ).+""");

        StringBuilder okFiles = new();
        StringBuilder badFilesByTags = new();
        StringBuilder badFilesByUrl = new();

        foreach (var mp3 in mp3s)
        {
            if (mp3.SampleRate >= 48_000)
            {
                // Check for URLs in the comments.
                if (!string.IsNullOrWhiteSpace(mp3.Comments) &&
                    urlRegex.Match(mp3.Comments) is Match match &&
                    match.Success)
                {
                    // URL found
                    badFilesByUrl.AppendLine($"{match.Value};{mp3.Path}");
                }
                else
                {
                    // No URL found, so noting a metadata summary.
                    badFilesByTags.AppendLine($"{string.Join(", ", mp3.Artists)}; {mp3.Album}; {mp3.Title}");
                }
            }
            else
            {
                okFiles.AppendLine($"{mp3.FileNameOnly} ({mp3.SampleRate}Hz)");
            }
        }

        try
        {
            File.WriteAllText("1-ok.log",       okFiles.ToString());
            File.WriteAllText("2-bad-tags.log", badFilesByTags.ToString());
            File.WriteAllText("3-bad-urls.log", badFilesByUrl.ToString());
        }
        catch (Exception ex)
        {
            printer.Error($"Error writing file: {ex.Message}");
        }

        printer.Print("Done with 3 log files written.");
    }
}
