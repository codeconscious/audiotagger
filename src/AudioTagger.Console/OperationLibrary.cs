using AudioTagger.Console.Operations;
using OperationFlags = System.Collections.Generic.HashSet<string>;
using System.Diagnostics.CodeAnalysis;
using AudioTagger.Library;

namespace AudioTagger.Console;

internal static class OperationLibrary
{
    private static readonly IReadOnlyList<Operation> Operations =
        [
            new(
                ["-v", "--view"],
                "View tag data.",
                new TagViewer()
            ),
            new(
                ["-vs", "--view-summary"],
                "View a tag data summary.",
                new TagViewerSummary()
            ),
            new(
                ["-u", "--update"],
                "Update tag data using filename patterns.",
                new TagUpdater()
            ),
            new(
                ["-u1", "--update-single"],
                "Update a single tag in multiple files to a single, manually-specified value.",
                new TagUpdaterSingle()
            ),
            new(
                ["-ug", "--update-genres"],
                "Update the genres in all files automatically using the artist-genre data in the settings.",
                new TagUpdaterGenreOnly()
            ),
            new(
                ["-um", "--update-multiple"],
                "Update a single tag with multiple values for multiple files.",
                new TagUpdaterMultiple()),
            new(
                ["-uy", "--update-year"],
                "Update the year using media files' own dates of creation. (Must do before other updates, lest the creation date be modified.)",
                new TagUpdaterYearOnly()
            ),
            new(
                ["-urt", "--reverse-track-numbers"],
                "Reverse the track numbers of the given files.",
                new TagUpdaterReverseTrackNumbers()
            ),
            new(
                ["-uea", "--extract-artwork"],
                "Extracts artwork from directory files if they have the same artist and album, then deletes the artwork from the files containing it.",
                new TagArtworkExtractor()
            ),
            new(
                ["-ura", "--remove-artwork"],
                "Removes artwork from files. (File size is not reduced.)",
                new TagArtworkRemover()
            ),
            new(
                ["-rt", "--rewrite-tags"],
                "Rewrites file tags. (Can be helping in reducing padding, such as from removed artwork.)",
                new TagRewriter()
            ),
            new(
                ["-r", "--rename"],
                "Rename and reorganize files into folders based on tag data.",
                new MediaFileRenamer()
            ),
            new(
                ["-d", "--duplicates"],
                "List tracks with identical artists and titles. No files are modified or deleted.",
                new TagDuplicateFinder()
            ),
            new(
                ["-s", "--stats"],
                "Display file statistics based on tag data.",
                new TagStats()
            ),
            new(
                ["-g", "--genres"],
                "Save the primary genre for each artist to a genre file.",
                new TagGenreExtractor()),
            new(
                ["-p", "--parse"],
                "Get a single tag value by parsing the data of another (generally Comments).",
                new TagParser()),
            new(
                ["--scan"],
                "Ad-hoc maintenance scanning work. (Not intended for normal use.)",
                new TagScanner(),
                isHidden: true),
            new(
                ["--cache-tags"],
                "Cache files' tag data locally to a JSON file whose path is specified in the settings. (Eventually, this will be helpful in speeding up certain operations.)",
                new TagCacher(),
                isHidden: true)
        ];

    public static Dictionary<string, string> GenerateHelpTextPairs(bool includeHidden)
    {
        var operations = includeHidden
            ? Operations
            : Operations.Where(o => !o.IsHidden).ToList();

        return operations.ToDictionary(
            o => string.Join(", " , o.Commands),
            o => o.Description);
    }

    private static Result<IPathOperation> GetPathOperation(string requestedOperation)
    {
        string loweredOperation = requestedOperation.ToLowerInvariant();

        IPathOperation? maybeOperation =
            Operations
                .FirstOrDefault(o => o.Commands.Contains(loweredOperation))?
                .PathOperation;

        return maybeOperation is null
            ? Result.Fail($"No valid operation for {requestedOperation} was found.")
            : Result.Ok(maybeOperation);
    }

    public static Result<ImmutableList<IPathOperation>> GetPathOperations(
        IEnumerable<string> requestedOperations)
    {
        var successes = new List<IPathOperation>();
        var failures = new List<string>();

        foreach (string operation in requestedOperations)
        {
            Result<IPathOperation> currentResult = GetPathOperation(operation);
            if (currentResult.IsSuccess)
            {
                successes.Add(currentResult.Value);
            }
            else
            {
                failures.Add(currentResult.Errors.First().Message);
            }
        }

        return failures.Count == 0
            ? Result.Ok(successes.ToImmutableList())
            : Result.Fail(string.Join(Environment.NewLine, failures));
    }

    internal sealed class Operation
    {
        public required OperationFlags Commands { get; init; }
        public required string Description { get; init; }
        public required IPathOperation PathOperation { get; init; }
        public required bool IsHidden { get; init; }

        private Operation() { }

        [SetsRequiredMembers]
        public Operation(
            OperationFlags commands,
            string description,
            IPathOperation pathOperation,
            bool isHidden = false)
        {
            (Commands, Description, PathOperation, IsHidden) =
            (commands, description, pathOperation, isHidden);
        }
    }
}
