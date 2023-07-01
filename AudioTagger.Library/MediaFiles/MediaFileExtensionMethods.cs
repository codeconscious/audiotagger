namespace AudioTagger.Library.MediaFiles;

public static class MediaFileExtensionMethods
{
    /// <summary>
    /// Joins a collection into one string using a specified separator string.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="separator"></param>
    /// <returns>A joined string. Never returns null.</returns>
    public static string Join(this IEnumerable<string> collection, string separator = "; ")
    {
        return collection is null
            ? string.Empty
            : string.Join(separator, collection);
    }

    /// <summary>
    /// Joins two collections into one formatted string. If their contents differ, then both will be included
    /// with the secondary collection placed within parentheses after the primary collection.
    /// </summary>
    /// <param name="primary">This collection is given priority.</param>
    /// <param name="secondary">This collection will not be added if it is identical to the primary one.</param>
    /// <param name="separator">Applies to each collection separately.</param>
    /// <returns>A combined string. Never returns null. Example: "primary 1; primary 2 (secondary 1; secondary 2)"</returns>
    public static string JoinWith(this IEnumerable<string> primary, IEnumerable<string> secondary, string separator = "; ")
    {
        if (primary is null && secondary is null)
            return string.Empty;

        string joiner(IEnumerable<string> collection) => string.Join(separator, collection);

        if (primary?.Any() != true)
            return joiner(secondary);

        if (secondary?.Any() != true)
            return joiner(primary);

        if (primary.Count() != secondary.Count())
            return $"{joiner(primary)} ({joiner(secondary)})";

        if (primary.Except(secondary).Any()) // Collections are not identical (despite equal counts)
            return $"{joiner(primary)} ({joiner(secondary)})";

        return joiner(primary); // Identical collection of equal length, so only print one
    }
}
