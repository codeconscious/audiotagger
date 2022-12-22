using System.Diagnostics;

namespace AudioTagger.Console;

public class Normalization : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IRegexCollection regexCollection,
                      IPrinter printer)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        var count = mediaFiles.Count;
        printer.Print($"There are {count} files for processing.");

        var startInfo = new ProcessStartInfo()
        {
            FileName = "find",
            Arguments = $". -name \"{workingDirectory}\\.*.mp3\" -exec mp3gain -r -k -p -s i {{}} \\;",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        var process = Process.Start(startInfo);

        while (!process.StandardOutput.EndOfStream)
        {
            string result = process.StandardOutput.ReadLine();
            // do something here
        }
        process.WaitForExit();
    }
}
