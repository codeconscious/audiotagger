﻿using AudioTagger.Library;
using Spectre.Console;
using System.Text.Json;
using static AudioTagger.Library.FSharp.IO;

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

        var (operationArg, pathArgs) = (args[0], args[1..].Distinct().ToImmutableHashSet());

        var operationResult = OperationFactory(operationArg);
        if (operationResult.IsFailed)
        {
            readSettingsResult.Errors.ForEach(x => printer.Error(x.Message));
            PrintInstructions(printer);
            return;
        }
        IPathOperation operation = operationResult.Value;

        var (validPaths, invalidPaths) = IOUtilities.GetFileGroups(pathArgs);

        if (invalidPaths.Any())
        {
            printer.Error($"{invalidPaths.Count} invalid path(s) found:");
            invalidPaths.ForEach(p => printer.Error($"- {p}"));
        }

        if (validPaths.Count == 0)
        {
            printer.Error("No valid paths were found, so cannot continue.");
            return;
        }

        foreach (PathItem path in validPaths)
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
        PathItem pathInfo,
        IPathOperation operation,
        Settings settings,
        IPrinter printer)
    {
        var (path, fileNames) = pathInfo switch
        {
            PathItem.Directory d => (d.Item1, d.Item2.ToList()),
            PathItem.File f      => (f.Item, [f.Item]),
            _ => throw new InvalidOperationException(""),
        };

        if (fileNames.Count == 0)
        {
            printer.Warning("No files were found, so will skip this path.");
            return;
        }

        printer.Print($"Processing {fileNames.Count} file(s) for path \"{path}\"...");

        Watch watch = new();

        var (mediaFiles, tagReadErrors) = ReadTagsShowingProgress(fileNames);

        int successes = fileNames.Count - tagReadErrors.Count;
        printer.Print($"Tags of {successes:#,##0} files read in {watch.ElapsedFriendly}.");

        if (tagReadErrors.Count != 0)
        {
            printer.Warning($"Tags could not be read for {tagReadErrors.Count} file(s).");

            int showCount = 10;
            tagReadErrors.Take(showCount).ToList().ForEach(printer.Error);
            if (tagReadErrors.Count > showCount)
                printer.Error($"plus {tagReadErrors.Count - showCount} more errors...");
        }

        try
        {
            operation.Start(mediaFiles, new DirectoryInfo(path), settings, printer);
        }
        catch (Exception ex)
        {
            printer.Error($"Error in while processing path \"{path}\": {ex.Message}");
            printer.PrintException(ex);
            return;
        }
    }

    private static (List<MediaFile>, List<string>) ReadTagsShowingProgress(ICollection<string> fileNames)
    {
        List<MediaFile> mediaFiles = new(fileNames.Count);
        List<string> tagReadErrors = [];

        AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
            })
            .Start(ctx =>
            {
                var task = ctx.AddTask("Reading tag data", maxValue: fileNames.Count);

                foreach (var fileName in fileNames)
                {
                    var readResult = MediaFile.ReadFileTags(fileName);
                    if (readResult.IsSuccess)
                    {
                        mediaFiles.Add(readResult.Value);
                    }
                    else
                    {
                        tagReadErrors.Add(readResult.Errors.First().Message);
                    }

                    task.Increment(1);
                }
            });

        return (mediaFiles, tagReadErrors);
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
}
