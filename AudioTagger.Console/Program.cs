global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace AudioTagger.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IPrinter printer = new SpectrePrinter();

            if (args.Length == 0)
            {
                PrintInstructions(printer);
                return;
            }

            var argQueue = new Queue<string>(args.Select(a => a.Trim()));

            // Select the desired operation using the first variable.
            IPathOperation? operation = OperationFactory(argQueue.Dequeue());

            const string regexPath = "../AudioTagger.Library/Regexes.txt";
            RegexCollection regexCollection;
            try
            {
                regexCollection = new RegexCollection(regexPath);
                printer.Print($"Found {regexCollection.Patterns.Count} regex(es).");
            }
            catch (FileNotFoundException)
            {
                printer.Error($"The file {regexPath} must exist.");
                return;
            }

            if (operation == null)
            {
                PrintInstructions(printer);
                return;
            }

            if (!argQueue.Any())
            {
                printer.Error("At least one file or directory path to process must be provided.");
                return;
            }

            foreach (var path in argQueue)
            {
                printer.Print($"Processing path \"{path}\"...");

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                IReadOnlyCollection<MediaFile> filesData;
                try
                {
                    filesData = MediaFile.PopulateFileData(path, searchSubDirectories: true);
                }
                catch (InvalidOperationException ex)
                {
                    printer.Error($"Path \"{path}\" could not be parsed: " + ex.Message);
                    continue;
                }

                if (!filesData.Any())
                {
                    printer.Error($"No files found at \"{path}\".");
                    continue;
                }

                // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
                // Reference: https://stackoverflow.com/q/5113750/11767771
                var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

                printer.Print($"Found {filesData.Count:#,##0} files in {elapsedMs:#,##0}ms.");

                var directoryInfo = new DirectoryInfo(path);

                operation.Start(filesData, directoryInfo, regexCollection, printer);
            }
        }

        /// <summary>
        /// Get the correct operation from the argument passed in.
        /// </summary>
        /// <param name="modeArg">The argument passed from the console.</param>
        /// <returns>A class for performing operations on files.</returns>
        private static IPathOperation? OperationFactory(string modeArg)
        {
            return OperationLibrary.GetPathOperation(modeArg);
        }

        private static void PrintInstructions(IPrinter printer)
        {
            printer.Print("ID3 audio tagger utilities.");
            printer.Print("Usage: ccaudiotagger [COMMAND] [FILES/DIRECTORIES]...", 0, 1, "");
            printer.Print("Supply one command, followed by one or more files or directories to process.", 0, 1, "");
            printer.Print("Commands:");
            printer.Print(OperationLibrary.GetHelpText());
        }
    }

    internal static class OperationLibrary
    {
        internal static readonly List<Operation> Operations = new()
        {
            new(new List<string>{"-v", "--view"}, "View tag data", new TagViewer()),
            new(new List<string>{"-vs", "--view-summary"}, "View a tag data summary", new TagSummaryViewer()),
            new(new List<string>{"-u", "--update"}, "Update tag data using filenames", new TagUpdater()),
            new(new List<string>{"-y", "--update-year"}, "Update years using Date Created years (Must do before other updates)", new TagUpdaterYearOnly()),
            new(new List<string>{"-r", "--rename"}, "Rename and reorganize files into folders based on tag data", new MediaFileRenamer()),
            new(new List<string>{"-d", "--duplicates"}, "List tracks with identical artists and titles (No files are deleted)", new TagDuplicateFinder()),
            new(new List<string>{"-s", "--stats"}, "Display file statistics using tag data", new TagStats()),
            new(new List<string>{"-m", "--manual"}, "Tentatively update specific tags manually using custom code, but without saving the updates", new ManualTagUpdater(saveUpdates: false)),
            new(new List<string>{"-mm", "--mmanual"}, "Update specific tags manually using custom code and save the updates", new ManualTagUpdater(saveUpdates: true)),
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
}
