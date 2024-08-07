using System.IO;
using System.Text.Json;
using FluentResults;

namespace AudioTagger.Library.UserSettings;

public static class SettingsService
{
    private const string _settingsFileName = "settings.json";

    /// <summary>
    /// Subversions of ID3 version 2 (such as 2.3 or 2.4).
    /// </summary>
    public enum Id3v2Version : byte
    {
        TwoPoint2 = 2,
        TwoPoint3 = 3,
        TwoPoint4 = 4,
    }

    /// <summary>
    /// Locks the ID3v2.x version to a valid one and optionally forces that version.
    /// </summary>
    /// <param name="version">The ID3 version 2 subversion to use.</param>
    /// <param name="forceAsDefault">
    ///     When true, forces the specified version when writing the file.
    ///     When false, will defer to the version within the file, if any.
    /// </param>
    public static void SetId3v2Version(Id3v2Version version, bool forceAsDefault)
    {
        TagLib.Id3v2.Tag.DefaultVersion = (byte)version;
        TagLib.Id3v2.Tag.ForceDefaultVersion = forceAsDefault;
    }

    /// <summary>
    /// Creates the specified settings file if it is missing.
    /// Otherwise, does nothing.
    /// </summary>
    /// <returns>A bool indicating success or no action (true) or else failure (false).</returns>
    public static bool CreateIfMissing(IPrinter printer)
    {
        if (File.Exists(_settingsFileName))
        {
            return true;
        }

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
    public static Result<Settings> Read(IPrinter printer, bool createFileIfMissing = false)
    {
        try
        {
            if (createFileIfMissing && !CreateIfMissing(printer))
            {
                return Result.Fail($"Settings file \"{_settingsFileName}\" missing.");
            }

            string text = File.ReadAllText(_settingsFileName);
            Settings json = JsonSerializer.Deserialize<Settings>(text)
                            ?? throw new JsonException();
            return Result.Ok(json);
        }
        catch (JsonException ex)
        {
            return Result.Fail($"Settings file JSON is invalid: {ex.Message}");
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
