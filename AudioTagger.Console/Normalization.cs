using System.Diagnostics;

namespace AudioTagger.Console;

/// <summary>
/// Normalize audio using ReplayGain.
/// </summary>
public sealed class Normalization : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        if (mediaFiles.Count == 0)
        {
            printer.Print("There are no files to normalize. Cancelling...");
            return;
        }

        printer.Print($"Found {mediaFiles.Count} file(s) for normalization.");

        foreach (MediaFile file in mediaFiles)
        {
            printer.Print($"- {file.Path}");

            ProcessStartInfo startInfo = new()
            {
                FileName = "mp3gain",
                Arguments = $"-r -k -p -s i \"{file.Path}\"",
                RedirectStandardOutput = true,
            };

            Process.Start(startInfo)!.WaitForExit();
        }
    }
}
