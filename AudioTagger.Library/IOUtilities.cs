using System.Text;

namespace AudioTagger;

public static class IOUtilities
{
    private static readonly List<string> SupportedExtensions =
        new() { ".mp3", ".ogg", ".mkv", ".mp4", ".m4a" };

    public static readonly Func<string, bool> IsSupportedFileExtension =
        new(
            fileName =>
                !string.IsNullOrWhiteSpace(fileName) &&
                !fileName.StartsWith(".") && // Unix-based OS hidden files
                SupportedExtensions.Any(ext =>
                    fileName.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)));

    /// <summary>
    /// Replaces characters that are invalid in file path names with a specified safe character.
    /// </summary>
    /// <returns>A corrected string or the original if no changes were needed.</returns>
    /// <remarks>TODO: Make a new class for this (e.g., FileUtilities, etc.).</remarks>
    public static string EnsurePathSafeString(string path, char replacementChar = '_')
    {
        return System.IO.Path.GetInvalidFileNameChars()
                    .ToList()
                    .Aggregate(
                        new StringBuilder(path),
                        (workingPath, invalidChar) =>
                            workingPath.Replace(invalidChar, replacementChar))
                    .ToString();
    }

    /// <summary>
    /// Concatenates multiple inputs to a string, then replaces characters that are
    /// invalid in file path names with a specified safe character.
    /// </summary>
    /// <returns>A corrected string or the original if no changes were needed.</returns>
    /// <remarks>TODO: Make a new class for this (e.g., FileUtilities, etc.).</remarks>
    public static string EnsurePathSafeString(
        IEnumerable<string> input,
        char replacementChar = '_',
        string joinWith = " && ")
    {
        return EnsurePathSafeString(
            string.Join(joinWith, input),
            replacementChar);
    }
}
