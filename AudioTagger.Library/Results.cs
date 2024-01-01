namespace AudioTagger;

public enum ResultType
{
    Neutral,
    Success,
    Failure,
    Cancelled,
    Unknown
}

public readonly record struct ResultProperties(ConsoleColor? Color, string Symbol = "");

public static class ResultsMap
{
    public static IReadOnlyDictionary<ResultType, ResultProperties> Map =>
        new Dictionary<ResultType, ResultProperties>
        {
            { ResultType.Neutral, new(ConsoleColor.DarkGray, "– ")},
            { ResultType.Success, new(ConsoleColor.DarkGreen, "✔︎ ")},
            { ResultType.Failure, new(ConsoleColor.DarkRed, "× ")},
            { ResultType.Cancelled, new(ConsoleColor.DarkRed, "＊ ")},
            { ResultType.Unknown, new(ConsoleColor.Blue, "? ")},
        };
}
