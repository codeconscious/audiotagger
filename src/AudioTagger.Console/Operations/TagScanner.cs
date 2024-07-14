using System.Text;
using System.Text.RegularExpressions;
using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

public sealed class TagScanner : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        Watch watch = new();

        printer.Print($"Found {mediaFiles.Count} audio files.");

        Regex playlistUrlRegex = new("""(?<=Playlist URL: ).+""");
        Regex videoUrlRegex = new("""(?<=URL: ).+""");

        StringBuilder okFiles = new();
        StringBuilder lowBitRateNoUrl = new();
        StringBuilder lowBitRateWithVideoUrl = new();
        StringBuilder lowBitRateWithPlaylistUrl = new();
        StringBuilder highSampleRateNoUrl = new();
        StringBuilder highSampleRateWithVideoUrl = new();
        StringBuilder highSampleRateWithPlaylistUrl = new();

        foreach (MediaFile file in mediaFiles)
        {
            if (file.SampleRate >= 48_000)
            {
                if (file.Comments.HasText() &&
                    playlistUrlRegex.Match(file.Comments) is Match playlistMatch &&
                    playlistMatch.Success)
                {
                    highSampleRateWithPlaylistUrl.AppendLine($"{playlistMatch.Value};{file.FileInfo}");
                }
                else if (file.Comments.HasText() &&
                         videoUrlRegex.Match(file.Comments) is Match videoMatch &&
                         videoMatch.Success)
                {
                    highSampleRateWithVideoUrl.AppendLine($"{videoMatch.Value};{file.FileInfo}");
                }
                else
                {
                    highSampleRateNoUrl.AppendLine($"{string.Join(", ", file.Artists)}; {file.Album}; {file.Title}; {file.FileInfo}");
                }
            }
            else if (file.BitRate < 110)
            {
                if (file.Comments.HasText() &&
                    playlistUrlRegex.Match(file.Comments) is Match playlistMatch &&
                    playlistMatch.Success)
                {
                    lowBitRateWithPlaylistUrl.AppendLine($"{playlistMatch.Value};{file.FileInfo}");
                }
                else if (file.Comments.HasText() &&
                         videoUrlRegex.Match(file.Comments) is Match videoMatch &&
                         videoMatch.Success)
                {
                    lowBitRateWithVideoUrl.AppendLine($"{videoMatch.Value};{file.FileInfo}");
                }
                else
                {
                    lowBitRateNoUrl.AppendLine($"{string.Join(", ", file.Artists)}; {file.Album}; {file.Title}; {file.FileInfo}");
                }
            }
            else
            {
                okFiles.AppendLine($"{file.FileNameOnly} [{file.SampleRate}Hz) @ {file.BitRate}kbps]");
            }
        }

        try
        {
            File.WriteAllText("results-ok.log", okFiles.ToString());
            File.WriteAllText("results-low-bit-rate-no-url.log", lowBitRateNoUrl.ToString());
            File.WriteAllText("results-low-bit-rate-video-url.log", lowBitRateWithVideoUrl.ToString());
            File.WriteAllText("results-low-bit-rate-playlist-url.log", lowBitRateWithPlaylistUrl.ToString());
            File.WriteAllText("results-high-sample-rate-no-url.log", highSampleRateNoUrl.ToString());
            File.WriteAllText("results-high-sample-rate-video-url.log", highSampleRateWithVideoUrl.ToString());
            File.WriteAllText("results-high-sample-rate-playlist-url.log", highSampleRateWithPlaylistUrl.ToString());
        }
        catch (Exception ex)
        {
            printer.Error($"Error writing files: {ex.Message}");
        }

        printer.Print($"Done in {watch.ElapsedFriendly}.");
    }
}
