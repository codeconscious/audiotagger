using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IPrinter printer = new ConsolePrinter();

            if (args.Length == 0)
            {
                PrintInstructions(printer);
                return;
            }

            var queuedArgs = new Queue<string>(args.Select(a => a.Trim()));

            // Select the desired operation using the first variable.
            IPathProcessor? processor = OperationFactory(queuedArgs.Dequeue());

            if (processor == null)
            {
                PrintInstructions(printer);
                return;
            }

            if (!queuedArgs.Any())
            {
                printer.Error("At least one file or directory path to work on must be provided.");
                return;
            }

            foreach (var path in queuedArgs)
            {
                IReadOnlyCollection<FileData> filesData;
                try
                {
                    filesData = FileData.PopulateFileData(path);
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

                processor.Start(filesData, printer);
            }
        }

        /// <summary>
        /// Get the correct operation from the argument passed in.
        /// </summary>
        /// <param name="modeArg"></param>
        /// <returns></returns>
        private static IPathProcessor? OperationFactory(string modeArg)
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