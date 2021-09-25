using System;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // IPrinter printer = new ConsolePrinter();
            IPrinter printer = new SpectrePrinter();

            if (args.Length == 0)
            {
                PrintInstructions(printer);
                return;
            }

            var queuedArgs = new Queue<string>(args.Select(a => a.Trim()));

            // Select the desired operation using the first variable.
            IPathOperation? operation = OperationFactory(queuedArgs.Dequeue());

            if (operation == null)
            {
                PrintInstructions(printer);
                return;
            }

            if (!queuedArgs.Any())
            {
                printer.Error("At least one file or directory path to process must be provided.");
                return;
            }

            foreach (var path in queuedArgs)
            {
                IReadOnlyCollection<MediaFile> filesData;
                try
                {
                    filesData = MediaFile.PopulateFileData(path);
                }
                catch (InvalidOperationException ex)
                {
                    printer.Error($"Path \"{path}\" could not be fully parsed: " + ex.Message);
                    continue;
                }

                if (!filesData.Any())
                {
                    printer.Error($"No files found at \"{path}\".");
                    continue;
                }

                operation.Start(filesData, printer);
            }
        }

        /// <summary>
        /// Get the correct operation from the argument passed in.
        /// </summary>
        /// <param name="modeArg"></param>
        /// <returns></returns>
        private static IPathOperation? OperationFactory(string modeArg)
        {
            return modeArg.ToLowerInvariant() switch
            {
                "-v" or "--view" => new TagViewer(),
                "-u" or "--update" => new TagUpdater(),
                "-r" or "--rename" => new FileRenamer(),
                _ => null
            };
        }

        private static void PrintInstructions(IPrinter printer)
        {
            printer.Print("Audio tagger and (eventually) renamer");
            printer.Print("Usage: jaudiotag [COMMAND] [FILES/DIRECTORIES]...", 0, 1, ""); // TODO: Decide on a name
            printer.Print("Supply one command, followed by one or more files or directories to process.", 0, 1, "");
            printer.Print("Commands:");
            printer.Print("   -v or --view   : View tags");
            printer.Print("   -u or --update : Update tags");
            printer.Print("   -r or --rename : Rename files based on tags (Coming soonish)");
            // TODO: Add option to disable colors
            // TODO: Make album art opt-in
        }
    }
}