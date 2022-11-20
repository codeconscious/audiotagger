using System.Diagnostics.CodeAnalysis;

namespace AudioTagger.Console;

public static class OperationLibrary
    {
        internal static readonly List<Operation> Operations = new()

     {
            new(
                new List<string>{"-v", "--view"},
                "View tag data",
                new TagViewer()
            ),
            new(
                new List<string>{"-vs", "--view-summary"},
                "View a tag data summary",
                new TagSummaryViewer()
            ),
            new(
                new List<string>{"-u", "--update"},
                "Update tag data using filenames",
                new TagUpdater()
            ),
            new(
                new List<string>{"-y", "--update-year"},
                "Update years using Date Created years (Must do before other updates)",
                new TagUpdaterYearOnly()
            ),
            new(
                new List<string>{"-r", "--rename"},
                "Rename and reorganize files into folders based on tag data",
                new MediaFileRenamer()
            ),
            new(
                new List<string>{"-d", "--duplicates"},
                "List tracks with identical artists and titles (No files are deleted)",
                new TagDuplicateFinder()
            ),
            new(
                new List<string>{"-s", "--stats"},
                "Display file statistics using tag data",
                new TagStats()
            ),
            new(
                new List<string>{"-m", "--manual"},
                "Test updating specific tags manually using custom code, but without saving the updates",
                new ManualTagUpdater(saveUpdates: false)
            ),
            new(
                new List<string>{"-mm", "--mmanual"},
                "Update specific tags manually using custom code and save the updates",
                new ManualTagUpdater(saveUpdates: true)),
        };

        public static string GetHelpText()
        {
            // TODO: Use Spectre to make a table.
            var output = new System.Text.StringBuilder();

            foreach (var operation in Operations)
            {
                output.Append("   ").AppendJoin(" or ", operation.Options).AppendLine();
                output.Append("      ").AppendLine(operation.Description);
            }

            return output.ToString();
        }

        public static IPathOperation? GetPathOperation(string requestedOperation)
        {
            return Operations.Where(o => o.Options.Contains(requestedOperation.ToLowerInvariant()))?
                             .SingleOrDefault()?
                             .PathOperation;
        }

        internal class Operation
        {
            public required List<string> Options { get;set;}
            public required string Description { get; set; }
            public required IPathOperation PathOperation { get; set; }

            private Operation() { }

            [SetsRequiredMembers]
            public Operation(List<string> options, string description, IPathOperation pathOperation)
            {
                Options = options;
                Description = description;
                PathOperation = pathOperation;
            }
        };
    }
