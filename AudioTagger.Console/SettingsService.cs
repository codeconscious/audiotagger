using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public static class SettingsService
{
    private const string _settingsFileName = "settings.json";

    public static bool EnsureSettingsFileExists(IPrinter printer)
    {
        if (File.Exists(_settingsFileName))
            return true;

        try
        {
            return WriteSettingsFile(new Settings());
        }
        catch (Exception ex)
        {
            printer.Error($"There was an error creating \"{_settingsFileName}\": {ex.Message}");
            return false;
        }
    }

    public static Settings? ReadSettings(IPrinter printer)
    {
        try
        {
            var text = File.ReadAllText(_settingsFileName);
            return JsonSerializer.Deserialize<Settings>(text);
        }
        catch (FileNotFoundException)
        {
            // printer.Print("Continuing with no settings since `settings.json` was not found. (See the readme file for more.)", appendLines: 1);
            return null;
        }
        catch (JsonException ex)
        {
            // printer.Print($"The settings file is invalid: {ex.Message}");
            // printer.Print("Continuing without settings...", appendLines: 1);
            return null;
        }
    }

    public static bool WriteSettingsFile(Settings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                });
            File.WriteAllText(_settingsFileName, json);
            return true;
        }
        catch (FileNotFoundException)
        {
            // printer.Print("Continuing with no settings since `settings.json` was not found. (See the readme file for more.)", appendLines: 1);
            // return null;
            throw new InvalidOperationException();
        }
        catch (JsonException ex)
        {
            // printer.Print($"The settings file is invalid: {ex.Message}");
            // printer.Print("Continuing without settings...", appendLines: 1);
            // return null;
            throw new InvalidOperationException();
        }
    }
}
