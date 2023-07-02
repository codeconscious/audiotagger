using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioTagger;

public sealed record Settings
{
    [JsonPropertyName("duplicates")]
    public Duplicates Duplicates { get; set; } = new();

    [JsonPropertyName("tagging")]
    public Tagging? Tagging { get; set; }

    [JsonPropertyName("renamePatterns")]
    public ImmutableList<string>? RenamePatterns { get; set; }

    [JsonPropertyName("artistGenres")]
    public Dictionary<string, string>? ArtistGenres { get; set; } = new();
}

public sealed record Duplicates
{
    [JsonPropertyName("titleReplacements")]
    public ImmutableList<string>? TitleReplacements { get; set; }
}

public sealed record Tagging
{
    [JsonPropertyName("regexPatterns")]
    public ImmutableList<string>? RegexPatterns { get; set; }
}

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
            return JsonSerializer.Deserialize<Settings>(text)
                   ?? throw new JsonException();
        }
        catch (FileNotFoundException)
        {
            printer.Error($"Settings file \"{_settingsFileName}\" was unexpectedly not found.");
            return null;
        }
        catch (JsonException ex)
        {
            printer.Error($"The settings file is invalid: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            printer.Error(ex.Message);
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
        }
        catch (JsonException ex)
        {
            printer.Error($"The settings file is invalid: {ex.Message}");
            return false;
        }
    }
}
