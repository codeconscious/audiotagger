global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.IO;
global using AudioTagger.Library.MediaFiles;
global using AudioTagger.Library.Settings;
global using FluentResults;
using Spectre.Console;
using System.Text.Json;

using VerifiedPaths = System.Collections.Immutable.ImmutableHashSet<string>;

namespace AudioTagger.Console;

public static class Program
{
    public static void Main(string[] args)
    {
        IPrinter printer = new SpectrePrinter();

        try
        {
            Run(args, printer);
        }
        catch (FileNotFoundException ex)
        {
            printer.Error($"Missing file error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            printer.Error($"JSON error: {ex.Message}");
        }
        catch (Exception ex)
        {
            printer.Error($"Unexpected error: {ex.Message}");
            AnsiConsole.WriteException(ex);
        }
    }

    private static void Run(string[] args, IPrinter printer)
    {
        if (args.Length < 2)
        {
            PrintInstructions(printer);
            return;
        }

        var readSettingsResult = SettingsService.Read(printer, createFileIfMissing: false);
        if (readSettingsResult.IsFailed)
        {
            printer.Error(readSettingsResult.Errors[0].Message);
            return;
        }
        Settings settings = readSettingsResult.Value;

        SettingsService.SetId3v2Version(
            version: SettingsService.Id3v2Version.TwoPoint3,
            forceAsDefault: true);

        var (operationArg, pathArgs) = (args[0], args[1..].Distinct().ToImmutableList());

        var operationResult = OperationFactory(operationArg);
        if (operationResult.IsFailed)
        {
            readSettingsResult.Errors.ForEach(x => printer.Error(x.Message));
            PrintInstructions(printer);
            return;
        }
        IPathOperation operation = operationResult.Value;

        foreach (string path in VerifyPaths(pathArgs))
        {
            try
            {
                ProcessPath(path, operation, settings, printer);
            }
            catch (InvalidOperationException ex)
            {
                printer.Error($"Error processing \"{path}\": {ex.Message}");
            }
        }
    }

    private static void ProcessPath(string path, IPathOperation operation, Settings settings, IPrinter printer)
    {
        printer.Print($"Processing path \"{path}\"...");

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        ImmutableList<MediaFile> mediaFiles;
        try
        {
            mediaFiles = MediaFile.PopulateFileData(path, searchSubDirectories: true);
        }
        catch (InvalidOperationException ex)
        {
            printer.Error($"Path \"{path}\" could not be parsed: " + ex.Message);
            return;
        }

        if (!mediaFiles.Any())
        {
            printer.Print("No files found.");
            return;
        }

        // Using ticks because .ElapsedMilliseconds was wildly inaccurate.
        // Reference: https://stackoverflow.com/q/5113750/11767771
        var elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;

        printer.Print($"Found {mediaFiles.Count:#,##0} files in {elapsedMs:#,##0}ms.");

        try
        {
            operation.Start(
                mediaFiles,
                new DirectoryInfo(path),
                printer,
                settings);
        }
        catch (Exception ex)
        {
            printer.Error($"Error in main operation: {ex.Message}");
            return;
        }
    }

    /// <summary>
    /// Get the correct operation from the argument passed in.
    /// </summary>
    /// <param name="modeArg">The argument passed from the console.</param>
    /// <returns>A class for performing operations on files.</returns>
    private static Result<IPathOperation> OperationFactory(string modeArg)
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
    /// A result containing a collection of verified paths that are expected to be valid
    /// if successful; otherwise, an error message.
    /// </summary>
    public static VerifiedPaths VerifyPaths(ICollection<string> maybePaths)
    {
        if (maybePaths?.Any() != true)
            throw new InvalidOperationException("No paths were passed in.");

        var invalidPaths = maybePaths.Where(p => !Path.Exists(p));
        if (invalidPaths.Any())
            throw new InvalidOperationException($"Invalid path(s): \"{string.Join("\" and \"", invalidPaths)}\".");

        return maybePaths.ToImmutableHashSet();
    }
}
