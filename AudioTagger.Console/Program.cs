global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.IO;
global using AudioTagger.Library.MediaFiles;
using Spectre.Console;

using VerifiedPaths = System.Collections.Immutable.ImmutableHashSet<string>;
using FluentResults;

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

        var settings = SettingsService.ReadSettings(printer, true);
        if (settings is null)
            return;

        var argQueue = new Queue<string>(args.Select(a => a.Trim()));

        // Select the desired operation using the first variable.
        IPathOperation? operation = OperationFactory(argQueue.Dequeue());

        if (operation == null)
        {
            PrintInstructions(printer);
            return;
        }

        var pathResult = VerifyPaths(argQueue.ToList());
        if (pathResult.IsFailed)
        {
            pathResult.Errors.ForEach(e => printer.Error(e.Message));
            return;
        }

        foreach (var path in pathResult.Value)
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
                printer.Error("No files found.");
                continue;
            }

            // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
            // Reference: https://stackoverflow.com/q/5113750/11767771
            var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

            printer.Print($"Found {filesData.Count:#,##0} files in {elapsedMs:#,##0}ms.");

            try
            {
                operation.Start(
                    filesData,
                    new DirectoryInfo(path),
                    printer,
                    settings);
            }
            catch (Exception ex)
            {
                printer.Error($"ERROR: {ex.Message}");
                return;
            }
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

        var table = new Table();
        table.AddColumns("Commands", "Descriptions");
        table.Border = TableBorder.Rounded;

        foreach (var pair in OperationLibrary.GenerateHelpTextPairs())
        {
            table.AddRow(pair.Key, pair.Value);
        }

        AnsiConsole.Write(table);

        printer.Print("Additionally, the file `settings.json` should be present in the application directory. " +
                      "A nearly-blank file will be automatically created if it does not exist. " +
                      "See the GitHub repository's readme file for more.",
                      prependLines: 1, appendLines: 1);
    }

    /// <summary>
    /// A result potentially containing a collection of verified paths that are expected to be valid.
    /// </summary>
    public static Result<VerifiedPaths> VerifyPaths(ICollection<string> maybePaths)
    {
        if (maybePaths?.Any() != true)
            return Result.Fail("No paths were passed in.");

        var invalid = maybePaths.Where(p => !Path.Exists(p));
        if (invalid.Any())
            return Result.Fail($"Invalid path(s): \"{string.Join("\" and \"", invalid)}\".");

        return Result.Ok(maybePaths.ToImmutableHashSet());
    }
}
