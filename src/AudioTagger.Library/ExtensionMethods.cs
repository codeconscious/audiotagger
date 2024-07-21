namespace AudioTagger.Library;

public static class ExtensionMethods
{
    /// <summary>
    /// Determines where a string is non-null and has text.
    /// </summary>
    public static bool HasText(this string str) => !string.IsNullOrWhiteSpace(str);
}
