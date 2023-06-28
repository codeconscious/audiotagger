using System.Text.Json;

namespace AudioTagger.Console;

public static class SettingsService
{
    private const string _settingsFileName = "settings.json";

    public static bool CreateIfMissing(IPrinter printer)
    {
        if (File.Exists(_settingsFileName))
            return true;

        try
        {
            return WriteSettingsToFile(new Settings(), printer);
        }
        catch (Exception ex)
        {
            printer.Error($"There was an error creating \"{_settingsFileName}\": {ex.Message}");
            return false;
        }
    }

    public static Settings? ReadSettings(IPrinter printer, bool createFileIfMissing = false)
    {
        try
        {
            if (createFileIfMissing && !CreateIfMissing(printer))
                return null;

            var text = File.ReadAllText(_settingsFileName);
            return JsonSerializer.Deserialize<Settings>(text) ?? throw new JsonException();
        }
        catch (FileNotFoundException)
        {
            printer.Print($"Settings file \"{_settingsFileName}\" was unexpectedly not found");
            return null;
        }
        catch (JsonException ex)
        {
            printer.Print($"The settings file is invalid: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            printer.Print($"ERROR: {ex.Message}");
            return null;
        }
    }

    public static bool WriteSettingsToFile(Settings settings, IPrinter printer)
    {
        try
        {
            var json = JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(
                        System.Text.Unicode.UnicodeRanges.All)
                });
            File.WriteAllText(_settingsFileName, json);
            return true;
        }
        catch (FileNotFoundException)
        {
            printer.Error($"Settings file \"{_settingsFileName}\" is missing.");
            return false;
            // throw new InvalidOperationException();
        }
        catch (JsonException ex)
        {
            printer.Print($"The settings file is invalid: {ex.Message}");
            return false;
            // throw new InvalidOperationException();
        }
    }
}
