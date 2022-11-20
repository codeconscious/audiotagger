global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.IO;

namespace AudioTagger.Console;

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
