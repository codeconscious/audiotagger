using System.Diagnostics.CodeAnalysis;
using OperationFlags = System.Collections.Generic.HashSet<string>;

namespace AudioTagger.Console;

internal static class OperationLibrary
{
    internal static readonly IReadOnlyList<Operation> Operations = new List<Operation>()
    {
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
        // new(
        //     ["-n", "--normalize", "--replaygain"],
        //     "Apply track normalization.",
        //     new Normalization()),
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
            ["--library"],
            "Update cached tag data to library file. (Still under development.)",
            new TagLibraryCacher(),
            isHidden: true),
    };

    public static Dictionary<string, string> GenerateHelpTextPairs(bool includeHidden)
    {
        IReadOnlyList<Operation> operations = includeHidden
            ? Operations
            : Operations.Where(o => !o.IsHidden).ToList();

        return operations.ToDictionary(
            o => string.Join(", " , o.Commands),
            o => o.Description);
    }

    public static Result<IPathOperation> GetPathOperation(string requestedOperation)
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

    internal sealed class Operation
    {
        public required OperationFlags Commands { get; init; }
        public required string Description { get; init; }
        public required IPathOperation PathOperation { get; init; }
        public required bool IsHidden { get; init; }

        private Operation() { }

        [SetsRequiredMembers]
        public Operation(OperationFlags options,
                         string description,
                         IPathOperation pathOperation,
                         bool isHidden = false)
        {
            Commands = options;
            Description = description;
            PathOperation = pathOperation;
            IsHidden = isHidden;
        }
    };
}
