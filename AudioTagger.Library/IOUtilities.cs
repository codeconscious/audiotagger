using System.Text;
using static AudioTagger.Library.FSharp.IO;
using FluentResults;

namespace AudioTagger.Library;

public static class IOUtilities
{
    /// <summary>
    /// Characters considered invalid for use in file paths.
    /// </summary>
    private static readonly char[] UnsafePathChars =
        [':', '?', '/', '⧸', '"', '|', '*'];

    /// <summary>
    /// Checks each of a collection of paths, returning the relevant data for valid ones and errors for invalid ones.
    /// </summary>
    public static (List<PathItem> Valid, List<PathItem> Invalid) GetFileGroups(ISet<string> paths)
    {
        if (paths?.Any() != true)
            return ([], []);

        var result = AudioTagger.Library.FSharp.IO.ReadPathFilenames([.. paths]);

        return
            (result.Where(i => !i.IsInvalid).ToList(),
             result.Where(i => i.IsInvalid).ToList());
    }

    /// <summary>
    /// Replaces invalid characters in file path names with a specified safe character.
    /// </summary>
    /// <returns>A corrected string or the original if no changes were needed.</returns>
    /// <remarks>It might be nice to allow specifying custom replacements for each invalid character.</remarks>
    public static string SanitizePath(string path, char replacementChar = '_')
    {
        var invalidChars = System.IO.Path.GetInvalidPathChars()
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
    public static string SanitizePath(
        IEnumerable<string> input,
        char replacementChar = '_',
        string joinWith = " && ")
    {
        return SanitizePath(
            string.Join(joinWith, input),
            replacementChar);
    }
}
