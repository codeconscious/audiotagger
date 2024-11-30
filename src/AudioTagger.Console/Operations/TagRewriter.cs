using AudioTagger.Library;

namespace AudioTagger.Console.Operations;

public sealed class TagRewriter : IPathOperation
{
    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        var watch = new Watch();
        var failures = 0;

        foreach (MediaFile file in mediaFiles)
        {
            var result = file.RewriteFileTags();

            if (result.IsFailed)
            {
                failures++;
                printer.FirstError(result);
            }
        }

        var failureLabel = failures == 1 ? "failure" : "failures";
        printer.Print($"File tags rewritten in {watch.ElapsedFriendly} with {failures} {failureLabel}.");
    }
}
