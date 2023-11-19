namespace AudioTagger.Console;

/// <summary>
/// Contains all of the path information for a specific MediaFile.
/// </summary>
internal sealed class MediaFilePathInfo
{
    private static readonly char[] PathSeparators = [
        Path.PathSeparator,
        Path.AltDirectorySeparatorChar,
        Path.DirectorySeparatorChar
    ];

    private string WorkingPath { get; init; }
    private List<string> SubDirectories { get; init; }
    private string FileName { get; init; }

    public MediaFilePathInfo(string workingPath, IEnumerable<string> subDirectories, string fileName)
    {
        WorkingPath = workingPath;
        SubDirectories = subDirectories.Where(sd => !string.IsNullOrWhiteSpace(sd)).ToList();
        FileName = fileName;
    }

    internal MediaFilePathInfo(string workingPath, string path)
    {
        if (path.Contains(workingPath))
        {
            WorkingPath = workingPath;

            string relativePath = path.Replace(workingPath, string.Empty);
            string[] parts = relativePath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            SubDirectories = [.. parts[..^1]]; // All but the final item.
            FileName = parts[^1]; // The final item, which, in paths, should always be the file name.
        }
        else
        {
            throw new ArgumentException($"Working path \"{workingPath}\" must be present in path \"{path}\".");
        }
    }

    /// <summary>
    /// Gets the full path, including the file name, optionally prepending the working directory.
    /// </summary>
    /// <param name="includeWorkingPath"></param>
    internal string FullFilePath(bool includeWorkingPath)
    {
        return includeWorkingPath
            ? Path.Combine([WorkingPath, ..SubDirectories, FileName])
            : Path.Combine([..SubDirectories, FileName]);
    }

    /// <summary>
    /// Gets the directory path, excluding the file name, optionally prepending the working directory.
    /// </summary>
    /// <param name="includeWorkingPath"></param>
    internal string DirectoryPath(bool includeWorkingPath)
    {
        return includeWorkingPath
            ? Path.Combine([WorkingPath, ..SubDirectories])
            : Path.Combine([..SubDirectories]);
    }
}
