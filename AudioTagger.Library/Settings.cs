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

    /// <summary>
    /// Creates the specified settings file if it is missing.
    /// Otherwise, does nothing.
    /// </summary>
    /// <param name="printer"></param>
    /// <returns>A bool indicating success or no action (true) or else failure (false).</returns>
    public static bool CreateIfMissing(IPrinter printer)
    {
        if (File.Exists(_settingsFileName))
            return true;

        try
        {
            return Write(new Settings(), printer);
        }
        catch (Exception ex)
        {
            printer.Error($"There was an error creating \"{_settingsFileName}\": {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reads the settings file and parses the JSON to a Settings object.
    /// </summary>
    public static Settings Read(IPrinter printer, bool createFileIfMissing = false)
    {
        try
        {
            if (createFileIfMissing && !CreateIfMissing(printer))
                throw new FileNotFoundException($"Settings file \"{_settingsFileName}\" missing.");

            var text = File.ReadAllText(_settingsFileName);
            return JsonSerializer.Deserialize<Settings>(text)
                   ?? throw new JsonException();
        }
        catch (FileNotFoundException)
        {
            throw new InvalidOperationException($"Settings file \"{_settingsFileName}\" not found.");
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Settings file JSON is invalid: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message);
        }
    }

    /// <summary>
    /// Write settings to the specified file.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="printer"></param>
    /// <returns>A bool indicating success or failure.</returns>
    public static bool Write(Settings settings, IPrinter printer)
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
