namespace AudioTagger.Console;

public static class ExtensionMethods
{
    /// <summary>
    /// Returns a bool indicating whether a string is not null and has text (true) or not.
    /// </summary>
    /// <remarks>I just got tired of `!string.IsNullOrWhiteSpace` everywhere...</remarks>
    public static bool HasText(this string? str) => !string.IsNullOrWhiteSpace(str);

    public static string? TextOrNull(this string? text) =>
        text switch
        {
            null or { Length: 0 } => null,
            _ => text
        };

    /// <summary>
    /// Parses a complete file path, returning the file's parent directory name.
    /// Example: "/me/Documents/audio/123.m4a" returns "audio".
    /// </summary>
    public static string? FileParentDirectory(this string fileName) =>
        Path.GetFileName(Path.GetDirectoryName(fileName) ?? null);
}
