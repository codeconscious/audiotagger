﻿using AudioTagger.Library;
using Spectre.Console;

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

        // Prefer ID3 v2.3 over v2.4 because the former is apparently more widely supported.
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

        var (validPaths, invalidPaths) = CheckPaths(pathArgs);

        if (invalidPaths.Any())
        {
            invalidPaths.ForEach(p => printer.Error($"The path \"{p}\" is invalid."));
        }

        if (!validPaths.Any())
        {
            printer.Error("No valid paths were found, so cannot continue.");
            return;
        }

        foreach (string path in validPaths)
        {
            try
            {
                ProcessPath(path, operation, settings, printer);
            }
            catch (Exception ex)
            {
                printer.Error($"Error processing \"{path}\": {ex.Message}");
            }
        }
    }

    private static void ProcessPath(
        string path,
        IPathOperation operation,
        Settings settings,
        IPrinter printer)
    {
        printer.Print($"Processing path \"{path}\"...");

        Watch watch = new();

        var fileNameResult = IOUtilities.GetAllFileNames(path, searchSubDirectories: true);
        if (fileNameResult.IsFailed)
        {
            printer.Error($"Could not read filenames for path \"{path}\"");
            return;
        }

        var fileNames = fileNameResult.Value;
        if (fileNames.IsEmpty)
        {
            printer.Error("No files were found, so will skip this path.");
            return;
        }
        printer.Print($"Found {fileNames.Length:#,##0} files in {watch.ElapsedFriendly}.");

        var populateResult = MediaFile.PopulateFileData(fileNameResult.Value);
        if (populateResult.IsFailed)
        {
            printer.Error($"No file tags were successfully read.");
            populateResult.Errors.Take(5).ToList().ForEach(e => printer.Error(e.Message));
            if (populateResult.Errors.Count > 5)
                printer.Error($"plus {populateResult.Errors.Count - 5} more errors...");
            return;
        }

        var (mediaFiles, errors) = populateResult.Value;
        if (errors.Any())
        {
            printer.Warning($"There were {errors.Count} error(s) reading file tags.");
        }

        printer.Print($"Read tags of {mediaFiles.Count:#,##0} files in {watch.ElapsedFriendly}.");

        try
        {
            operation.Start(mediaFiles, new DirectoryInfo(path), settings, printer);
        }
        catch (Exception ex)
        {
            printer.Error($"Error in main operation: {ex.Message}");
            printer.PrintException(ex);
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
        printer.Print("Usage: ccaudiotagger [COMMAND] [FILES/DIRECTORIES]...", 0, 1, string.Empty);
        printer.Print("Supply one command, followed by one or more files or directories to process.", 0, 1, string.Empty);

        Table table = new();
        table.AddColumns("Commands", "Descriptions");
        table.Border = TableBorder.Rounded;

        foreach (KeyValuePair<string, string> pair in OperationLibrary.GenerateHelpTextPairs(includeHidden: false))
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
    /// Checks each of a collection of paths, returning the valid and invalid ones as tuple members.
    /// </summary>
    private static (ImmutableList<string> Valid, ImmutableList<string> Invalid) CheckPaths(
        ICollection<string> paths)
    {
        if (paths?.Any() != true)
            return new ([], []);

        List<string> valid = [];
        List<string> invalid = [];

        foreach (string path in paths)
        {
            if (Path.Exists(path))
                valid.Add(path);
            else
                invalid.Add(path);
        }

        return new ([.. valid], [.. invalid]);
    }
}
