using System.Diagnostics.CodeAnalysis;

namespace AudioTagger.Console;

internal static class OperationLibrary
{
    internal static readonly IReadOnlyList<Operation> Operations = new List<Operation>()
    {
        new(
            new List<string>{"-v", "--view"},
            "View tag data.",
            new TagViewer()
        ),
        new(
            new List<string>{"-vs", "--view-summary"},
            "View a tag data summary.",
            new TagSummaryViewer()
        ),
        new(
            new List<string>{"-u", "--update"},
            "Update tag data using filename patterns.",
            new TagUpdater()
        ),
        new(
            new List<string>{"-u1", "--update-single"},
            "Update a single tag by manually specifying a value",
            new TagUpdaterSingle()
        ),
        new(
            new List<string>{"-y", "--update-year"},
            "Update years using Date Created from file properties. (Must do before other updates.)",
            new TagUpdaterYearOnly()
        ),
        new(
            new List<string>{"-r", "--rename"},
            "Rename and reorganize files into folders based on tag data.",
            new MediaFileRenamer()
        ),
        new(
            new List<string>{"-d", "--duplicates"},
            "List tracks with identical artists and titles. No files are deleted.",
            new TagDuplicateFinder()
        ),
        new(
            new List<string>{"-s", "--stats"},
            "Display file statistics using tag data.",
            new TagStats()
        ),
        new(
            new List<string>{"-n", "--normalize", "--replaygain"},
            "Apply track normalization.",
            new Normalization()),
    };

    public static Dictionary<string, string> GenerateHelpTextPairs()
    {
        return Operations.ToDictionary(
            o => string.Join(", " , o.Commands),
            o => o.Description);
    }

    public static IPathOperation? GetPathOperation(string requestedOperation)
    {
        return Operations.Where(o => o.Commands.Contains(requestedOperation.ToLowerInvariant()))?
                         .SingleOrDefault()?
                         .PathOperation;
    }

    internal class Operation
    {
        public required List<string> Commands { get;set;}
        public required string Description { get; set; }
        public required IPathOperation PathOperation { get; set; }

        private Operation() { }

        [SetsRequiredMembers]
        public Operation(List<string> options, string description, IPathOperation pathOperation)
        {
            Commands = options;
            Description = description;
            PathOperation = pathOperation;
        }
    };
}
