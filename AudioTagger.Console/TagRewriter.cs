namespace AudioTagger.Console;

public sealed class TagRewriter : IPathOperation
{
    public void Start(
        IReadOnlyCollection<MediaFile> filesData,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        var watch = new Watch();
        var failures = 0;

        Result result;
        foreach (MediaFile file in filesData)
        {
            result = file.RewriteFileTags();
            if (result.IsFailed)
            {
                printer.Error(result.Errors.First().Message);
            }
        }

        var failureLabel = failures == 1 ? "failure" : "failures";
        printer.Print($"File tags rewritten in {watch.ElapsedFriendly} with {failures} {failureLabel}.");
    }
}
