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
    /// with the second collection placed within parentheses after the first collection.
    /// </summary>
    /// <param name="first">This collection is given priority.</param>
    /// <param name="second">This collection will not be added if it is identical to the primary one.</param>
    /// <param name="separator">Applies to each collection separately.</param>
    /// <returns>A combined string. Never returns null. Example: "first1; first2 (second1; second2)"</returns>
    public static string JoinWith(this IEnumerable<string> first, IEnumerable<string> second, string separator = "; ")
    {
        if (first is null && second is null)
            return string.Empty;

        string joinerFunc(IEnumerable<string> collection) => string.Join(separator, collection);

        if (first?.Any() != true)
            return joinerFunc(second);

        if (second?.Any() != true)
            return joinerFunc(first);

        if (first.Count() != second.Count())
            return $"{joinerFunc(first)} ({joinerFunc(second)})";

        return joinerFunc(first); // Identical collections of equal length, so only print the first.
    }
}
