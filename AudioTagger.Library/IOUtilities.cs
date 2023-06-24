using System.Text;

namespace AudioTagger;

public static class IOUtilities
{
    // TODO: Change into a setting of supported file extensions.
    public static readonly Func<string, bool> IsSupportedFileExtension =
        new(file => !string.IsNullOrWhiteSpace(file) &&
                    !file.StartsWith(".") &&
                    (file.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase) ||
                     file.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase) ||
                     file.EndsWith(".mkv", StringComparison.InvariantCultureIgnoreCase) ||
                     file.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase) ||
                     file.EndsWith(".m4a", StringComparison.InvariantCultureIgnoreCase)));

    /// <summary>
    /// Replaces characters that are invalid in file path names with a specified safe character.
    /// </summary>
    /// <returns>A corrected string or the original if no changes were needed.</returns>
    /// <remarks>TODO: Make a new class for this (e.g., FileUtilities, etc.).</remarks>
    public static string EnsurePathSafeString(string input, char replacementChar = '_')
    {
        return System.IO.Path.GetInvalidFileNameChars()
                    .ToList()
                    .Aggregate(
                        new StringBuilder(input),
                        (workingString, invalidChar) =>
                            workingString.Replace(invalidChar, replacementChar))
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
