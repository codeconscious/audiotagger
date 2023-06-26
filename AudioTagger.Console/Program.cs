global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.IO;
using Spectre.Console;
using System.Text.Json;

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

        const string settingsFileName = "settings.json";
        if (!EnsureSettingsFileExists(settingsFileName, printer)) return;
        var settings = ReadSettings(settingsFileName, printer);

        var argQueue = new Queue<string>(args.Select(a => a.Trim()));

        // Select the desired operation using the first variable.
        IPathOperation? operation = OperationFactory(argQueue.Dequeue());

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

    private static Settings? ReadSettings(string fileName, IPrinter printer)
    {
        try
        {
            var text = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<Settings>(text);
        }
        catch (FileNotFoundException)
        {
            printer.Print("Continuing with no settings since `settings.json` was not found. (See the readme file for more.)", appendLines: 1);
            return null;
        }
        catch (JsonException ex)
        {
            printer.Print($"The settings file is invalid: {ex.Message}");
            printer.Print("Continuing without settings...", appendLines: 1);
            return null;
        }
    }

    private static bool EnsureSettingsFileExists(string fileName, IPrinter printer)
    {
        if (File.Exists(fileName))
            return true;

        try
        {
            var json = JsonSerializer.Serialize(new Settings(),
                                                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, json);
            printer.Print($"Created empty settings file \"{fileName}\" successfully.");
            return true;
        }
        catch (Exception ex)
        {
            printer.Error($"There was an error creating \"{fileName}\": {ex.Message}");
            return false;
        }
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
}
