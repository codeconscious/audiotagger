global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.IO;

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
            return modeArg.ToLowerInvariant() switch
            {
                "-v" or "--view" or "--view-details" => new TagViewer(),
                "-vs" or "--view-summary" => new TagSummaryViewer(),
                "-u" or "--update" => new TagUpdater(),
                "-y" or "--update-year" => new TagUpdaterYearOnly(),
                "-r" or "--rename" => new MediaFileRenamer(),
                "-d" or "--duplicates" => new TagDuplicateFinder(),
                "-s" or "--stats" => new TagStats(),
                "-m" or "--manual" => new ManualTagUpdater(saveUpdates: false),
                "-mm" or "--mmanual" => new ManualTagUpdater(saveUpdates: true),
                _ => null
            };
        }

        private static void PrintInstructions(IPrinter printer)
        {
            // TODO: Might be worth combining this data with the switch statement above.
            printer.Print("ID3 audio tagger utilities.");
            printer.Print("Usage: ccaudiotagger [COMMAND] [FILES/DIRECTORIES]...", 0, 1, "");
            printer.Print("Supply one command, followed by one or more files or directories to process.", 0, 1, "");
            printer.Print("Commands:");
            printer.Print("   -v  or --view         : View tag data");
            printer.Print("   -vs or --view-summary : View a tag data summary");
            printer.Print("   -u  or --update       : Update tag data using filenames");
            printer.Print("   -y  or --update-year  : Update years using Date Created years (Must do before other updates)");
            printer.Print("   -r  or --rename       : Rename and reorganize files into folders based on tagdata ");
            printer.Print("   -d  or --duplicates   : List tracks with identical artists and titles (No files are deleted)");
            printer.Print("   -s  or --stats        : Display file statistics using tag data");
            printer.Print("   -m  or --manual       : Update specific tags manually using custom code without saving the updates");
            printer.Print("   -mm or --mmanual      : Update specific tags manually using custom code and save the updates");
        }
    }
}
