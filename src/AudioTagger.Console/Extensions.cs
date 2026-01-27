namespace AudioTagger.Console;

public static class Extensions
{
    extension<T>(IEnumerable<T> collection)
    {
        public bool None() =>
            !collection.Any();

        public bool None(Func<T, bool> predicate) =>
            !collection.Any(predicate);
    }

    extension(string? str)
    {
        public bool HasText() => !string.IsNullOrWhiteSpace(str);

        public string? TextOrNull() =>
            str switch
            {
                null or { Length: 0 } => null,
                _ => str
            };
    }
}
