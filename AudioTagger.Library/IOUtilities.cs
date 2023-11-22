using System.Text;

namespace AudioTagger;

public static class IOUtilities
{
    private static readonly List<string> SupportedExtensions =
        [".mp3", ".ogg", ".mkv", ".mp4", ".m4a"];

    /// <summary>
    /// Characters considered invalid for use in file paths.
    /// </summary>
    private static readonly char[] UnsafePathChars =
        [':', '?', '/', '"'];

    public static readonly Func<string, bool> IsSupportedFileExtension =
        new(
            fileName =>
                !string.IsNullOrWhiteSpace(fileName) &&
                !fileName.StartsWith(".") && // Unix-based OS hidden files
                SupportedExtensions.Any(ext =>
                    fileName.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)));

    /// <summary>
    /// Replaces invalid characters in file path names with a specified safe character.
    /// </summary>
    /// <returns>A corrected string or the original if no changes were needed.</returns>
    /// <remarks>It might be nice to allow specifying custom replacements for each invalid character.</remarks>
    public static string SanitizePath(string path, char replacementChar = '_')
    {
        IEnumerable<char> invalidChars = System.IO.Path.GetInvalidPathChars()
                                .Concat(System.IO.Path.GetInvalidFileNameChars())
                                .Concat(UnsafePathChars);

        return invalidChars
                    .Aggregate(
                        new StringBuilder(path),
                        (workingPath, invalidChar) =>
                            workingPath.Replace(invalidChar, replacementChar))
                    .ToString();
    }

    /// <summary>
    /// Concatenates multiple inputs to a string with a given separator, then replaces invalid
    /// characters in file path names with a specified safe character.
    /// </summary>
    /// <returns>A corrected string or the original if no changes were needed.</returns>
    public static string SanitizePath(IEnumerable<string> input,
                                      char replacementChar = '_',
                                      string joinWith = " && ")
    {
        return SanitizePath(
            string.Join(joinWith, input),
            replacementChar);
    }
}
