namespace AudioTagger.Library.MediaFiles;

public static class MediaFileExtensionMethods
{
    extension(IEnumerable<string> first)
    {
        /// <summary>
        /// Joins a collection into one string using a specified separator string.
        /// </summary>
        /// <param name="separator"></param>
        /// <returns>A joined string. Never returns null.</returns>
        public string Join(string separator = "; ")
        {
            return string.Join(separator, first);
        }

        /// <summary>
        /// Joins two collections into one formatted string. If their contents differ, then both will be included
        /// with the second collection placed within parentheses after the first collection.
        /// </summary>
        /// <param name="second">This collection will not be added if it is identical to the primary one.</param>
        /// <param name="separator">Applies to each collection separately.</param>
        /// <returns>A combined string. Never returns null. Example: "first1; first2 (second1; second2)"</returns>
        public string JoinWith(IEnumerable<string> second, string separator = "; ")
        {
            string Joiner(IEnumerable<string> collection1) => string.Join(separator, collection1);

            if (!first.Any())
                return Joiner(second);

            if (!second.Any())
                return Joiner(first);

            if (first.Count() != second.Count())
                return $"{Joiner(first)} ({Joiner(second)})";

            return Joiner(first); // Identical collections of equal length, so only print the first.
        }
    }

    /// <summary>
    /// Combines two string collections to a string representation.
    /// If they are identical or if only one collection has items,
    /// only one will be included, as-is. If both have differing items,
    /// then both will be included, with the secondary collection's
    /// items will be enclosed in parentheses.
    /// </summary>
    public static string JoinWith(
        this IList<string> primary,
        IList<string> secondary,
        string separator)
    {
        static string Format(IList<string> artists, string separator) =>
            string.Join(separator, artists);

        return (primary, secondary) switch
        {
            ([..], [])   => $"{Format(primary, separator)}",
            ([], [..])   => $"{Format(secondary, separator)}",
            ([..], [..]) when primary.All(secondary.Contains)
                           && primary.Count == secondary.Count
                         => $"{Format(primary, separator)}",
            ([..], [..]) => $"{Format(primary, separator)} ({Format(secondary, separator)})",
            _ => string.Empty
        };
    }
}
