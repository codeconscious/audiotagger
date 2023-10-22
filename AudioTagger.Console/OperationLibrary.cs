using System.Diagnostics.CodeAnalysis;
using OperationFlags = System.Collections.Generic.List<string>;

namespace AudioTagger.Console;

internal static class OperationLibrary
{
    internal static readonly IReadOnlyList<Operation> Operations = new List<Operation>()
    {
        new(
            new OperationFlags{"-v", "--view"},
            "View tag data.",
            new TagViewer()
        ),
        new(
            new OperationFlags{"-vs", "--view-summary"},
            "View a tag data summary.",
            new TagViewerSummary()
        ),
        new(
            new OperationFlags{"-u", "--update"},
            "Update tag data using filename patterns.",
            new TagUpdater()
        ),
        new(
            new OperationFlags{"-u1", "--update-single"},
            "Update a single tag by manually specifying a value",
            new TagUpdaterSingle()
        ),
        new(
            new OperationFlags{"-m", "--multiple"},
            "Update a single tag with multiple values for multiple files.",
            new TagUpdaterMultiple()),
        new(
            new OperationFlags{"-y", "--update-year"},
            "Update the year using media file's own dates of creation. (Must do before other updates.)",
            new TagUpdaterYearOnly()
        ),
        new(
            new OperationFlags{"-r", "--rename"},
            "Rename and reorganize files into folders based on tag data.",
            new MediaFileRenamer()
        ),
        new(
            new OperationFlags{"-d", "--duplicates"},
            "List tracks with identical artists and titles. No files are modified or deleted.",
            new TagDuplicateFinder()
        ),
        new(
            new OperationFlags{"-s", "--stats"},
            "Display file statistics based on tag data.",
            new TagStats()
        ),
        new(
            new OperationFlags{"-n", "--normalize", "--replaygain"},
            "Apply track normalization.",
            new Normalization()),
        new(
            new OperationFlags{"-g", "--genres"},
            "Save the primary genre for each artist to the settings file.",
            new GenreExtractor()),
        new(
            new OperationFlags{"-p", "--parse"},
            "Get a single tag value by parsing the data of another (generally Comments).",
            new TagParser()),
        new(
            new OperationFlags{"--scan"},
            "Maintenance scanning work. (Not intended for normal use.)",
            new ScanTags(),
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
        var loweredOperation = requestedOperation.ToLowerInvariant();
        var maybeOperation = Operations.FirstOrDefault(o => o.Commands.Contains(loweredOperation))?
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
