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
            new OperationFlags{"-y", "--update-year"},
            "Update years using Date Created from file properties. (Must do before other updates.)",
            new TagUpdaterYearOnly()
        ),
        new(
            new OperationFlags{"-r", "--rename"},
            "Rename and reorganize files into folders based on tag data.",
            new MediaFileRenamer()
        ),
        new(
            new OperationFlags{"-d", "--duplicates"},
            "List tracks with identical artists and titles. No files are deleted.",
            new TagDuplicateFinder()
        ),
        new(
            new OperationFlags{"-s", "--stats"},
            "Display file statistics using tag data.",
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
    };

    public static Dictionary<string, string> GenerateHelpTextPairs()
    {
        return Operations.ToDictionary(
            o => string.Join(", " , o.Commands),
            o => o.Description);
    }

    public static IPathOperation GetPathOperation(string requestedOperation)
    {
        return Operations.Where(o => o.Commands.Contains(requestedOperation.ToLowerInvariant()))?
                         .SingleOrDefault()?
                         .PathOperation
               ?? throw new InvalidOperationException("Invalid operation requested.");
    }

    internal sealed class Operation
    {
        public required OperationFlags Commands { get; set;}
        public required string Description { get; set; }
        public required IPathOperation PathOperation { get; set; }

        private Operation() { }

        [SetsRequiredMembers]
        public Operation(OperationFlags options,
                         string description,
                         IPathOperation pathOperation)
        {
            Commands = options;
            Description = description;
            PathOperation = pathOperation;
        }
    };
}
