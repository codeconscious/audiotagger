using AudioTagger.Library;
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
            printer.FirstError(readSettingsResult);
            return;
        }
        Settings settings = readSettingsResult.Value;

        // Prefer ID3 v2.3 over v2.4 because the former is apparently more widely supported.
        SettingsService.SetId3v2Version(
            version: SettingsService.Id3v2Version.TwoPoint3,
            forceAsDefault: true);

        var operationArgs = args.TakeWhile(a => a.StartsWith('-')).ToImmutableArray();
        var pathArgs = args[operationArgs.Length..];

        var operationResult = OperationFactory(operationArgs);
        if (operationResult.IsFailed)
        {
            readSettingsResult.Errors.ForEach(x => printer.Error(x.Message));
            PrintInstructions(printer);
            return;
        }
        var operations = operationResult.Value;

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
                ProcessPath(path, operations, settings, printer);
            }
            catch (Exception ex)
            {
                printer.Error($"Error processing \"{path}\": {ex.Message}");
            }
        }
    }

    private static void ProcessPath(
        string path,
        ImmutableList<IPathOperation> operations,
        Settings settings,
        IPrinter printer)
    {
        printer.Print($"Processing path \"{path}\"...");

        Watch watch = new();

        var fileNameResult = IOUtilities.GetAllFileNames(path, searchSubDirectories: true);
        if (fileNameResult.IsFailed)
        {
            printer.FirstError(fileNameResult, "Error reading filenames for path \"{path}\":");
            return;
        }

        ImmutableArray<string> fileNames = fileNameResult.Value;
        if (fileNames.IsEmpty)
        {
            printer.Warning($"No files were found in \"{path}\".");
            return;
        }

        printer.Print($"Found {fileNames.Length:#,##0} files in {watch.ElapsedFriendly}.");

        var (mediaFiles, tagReadErrors) = ReadTagsShowingProgress(fileNames);

        int successes = fileNames.Length - tagReadErrors.Count;
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
            foreach (var operation in operations)
            {
                operation.Start(mediaFiles, new DirectoryInfo(path), settings, printer);
            }
        }
        catch (Exception ex)
        {
            printer.Error($"Error in while processing path \"{path}\": {ex.Message}");
            printer.PrintException(ex);
            return;
        }
    }

    private static (List<MediaFile>, List<string>) ReadTagsShowingProgress(
        ICollection<string> fileNames)
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
    /// <param name="modeArgs">The argument passed from the console.</param>
    /// <returns>A class for performing operations on files.</returns>
    private static Result<ImmutableList<IPathOperation>> OperationFactory(IEnumerable<string> modeArgs)
    {
        return OperationLibrary.GetPathOperations(modeArgs);
    }

    private static void PrintInstructions(IPrinter printer)
    {
        printer.Print("ID3 audio tagger utilities.");
        printer.Print("Usage: dotnet run -- [COMMAND(s)] [FILES/DIRECTORIES]...", 0, 1, string.Empty);
        printer.Print("Supply one or more commands, followed by one or more files or directories to process.", 0, 1, string.Empty);

        Table table = new();
        table.AddColumns("Commands", "Descriptions");
        table.Border = TableBorder.Rounded;

        var helpPairs = OperationLibrary.GenerateHelpTextPairs(includeHidden: false);
        foreach (KeyValuePair<string, string> pair in helpPairs)
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
