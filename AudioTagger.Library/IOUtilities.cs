using System.Text;
using FluentResults;

namespace AudioTagger.Library;

public static class IOUtilities
{
    private static readonly List<string> SupportedExtensions =
        [".mp3", ".ogg", ".mkv", ".mp4", ".m4a"];

    /// <summary>
    /// Characters considered invalid for use in file paths.
    /// </summary>
    private static readonly char[] UnsafePathChars =
        [':', '?', '/', '⧸', '"', '|', '*'];

    public static readonly Func<string, bool> IsSupportedFileExtension =
        new(
            fileName =>
                fileName.HasText() &&
                !fileName.StartsWith(".") && // Unix-based OS hidden files
                SupportedExtensions.Any(ext =>
                    fileName.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)));

    public static Result<ImmutableArray<string>> GetAllFileNames(string path, bool searchSubDirectories)
    {
        try
        {
            if (System.IO.Directory.Exists(path))
            {
                var files = System.IO.Directory
                    .EnumerateFiles(
                        path,
                        "*.*",
                        searchSubDirectories
                            ? System.IO.SearchOption.AllDirectories
                            : System.IO.SearchOption.TopDirectoryOnly)
                    .Where(IsSupportedFileExtension)
                    .ToImmutableArray();
                return Result.Ok(files);
            }

            if (System.IO.File.Exists(path))
            {
                var file = new string[] { path }.ToImmutableArray();
                return Result.Ok(file);
            }

            throw new ArgumentException($"The path \"{path}\" was invalid.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

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
