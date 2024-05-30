using System.IO;
using System.Text.Json;
using FluentResults;

namespace AudioTagger.Library.Settings;

public static class SettingsService
{
    private const string _settingsFileName = "settings.json";

    private static readonly JsonSerializerOptions _jsonSerializerOptions =
        new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(
                System.Text.Unicode.UnicodeRanges.All)
        };

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
    /// Reads the settings file and parses the JSON to a Settings object.
    /// </summary>
    public static Result<Settings> Read(bool createFileIfMissing = false)
    {
        try
        {
            if (createFileIfMissing)
            {
                var result = CreateNewIfMissing();
                if (result.IsFailed)
                {
                    return result;
                }
            }

            var json = File.ReadAllText(_settingsFileName);
            Settings settings = JsonSerializer.Deserialize<Settings>(json)
                                ?? throw new JsonException();
            return Result.Ok(settings);
        }
        catch (JsonException ex)
        {
            return Result.Fail($"Settings file JSON is invalid: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates the specified settings file if it is missing.
    /// Otherwise, does nothing.
    /// </summary>
    /// <returns>A bool indicating success or no action (true) or else failure (false).</returns>
    public static Result CreateNewIfMissing()
    {
        return File.Exists(_settingsFileName)
            ? Result.Ok()
            : Write(Settings.CreateEmpty());
    }

    /// <summary>
    /// Write settings to the specified file.
    /// </summary>
    public static Result Write(Settings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonSerializerOptions);
            File.WriteAllText(_settingsFileName, json);
            return Result.Ok();
        }
        catch (JsonException ex)
        {
            return Result.Fail($"Failed to serialize the settings: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
