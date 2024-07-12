namespace AudioTagger.Library;

public static class ExtensionMethods
{
    /// <summary>
    /// Returns a bool indicating whether a string is not null and has text (true) or not.
    /// </summary>
    /// <remarks>I just got tired of `!string.IsNullOrWhiteSpace` everywhere...</remarks>
    public static bool HasText(this string str) => !string.IsNullOrWhiteSpace(str);
}
