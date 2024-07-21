namespace AudioTagger.Console;

public static class Extensions
{
    /// <summary>
    /// Determines whether a collection is empty.
    /// </summary>
    public static bool None<T>(this IEnumerable<T> collection) =>
        !collection.Any();

    /// <summary>
    /// Determines whether no elements of a sequence satisfy a given condition.
    /// </summary>
    public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> predicate) =>
        !collection.Any(predicate);

    /// <summary>
    /// Returns a bool indicating whether a string is not null and has text (true) or not.
    /// </summary>
    public static bool HasText(this string? str) => !string.IsNullOrWhiteSpace(str);

    public static string? TextOrNull(this string? text) =>
        text switch
        {
            null or { Length: 0 } => null,
            _ => text
        };
}
