using System.Diagnostics;

namespace AudioTagger.Console;

/// <summary>
/// Normalize audio using ReplayGain.
/// </summary>
public class Normalization : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings? settings = null)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to normalize. Cancelling...");
            return;
        }

        printer.Print($"Found {mediaFiles.Count} file(s) for normalization.");

        foreach (var file in mediaFiles)
        {
            printer.Print($"- {file.Path}");

            var startInfo = new ProcessStartInfo()
            {
                FileName = "mp3gain",
                Arguments = $"-r -k -p -s i \"{file.Path}\"",
                RedirectStandardOutput = true,
            };

            Process.Start(startInfo)!.WaitForExit();
        }
    }
}
