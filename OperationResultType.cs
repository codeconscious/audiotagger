using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public enum ResultType
    {
        Neutral,
        Success,
        Failure,
        Cancelled,
        Unknown
    }

    public record ResultProperties(ConsoleColor? Color, string Symbol = "");

    public static class ResultsMap
    {
        public static IReadOnlyDictionary<ResultType, ResultProperties> Map =>
            new Dictionary<ResultType, ResultProperties>
            {
                { ResultType.Neutral, new ResultProperties(null, "– ")},
                { ResultType.Success, new ResultProperties(ConsoleColor.DarkGreen, "✔︎ ")},
                { ResultType.Failure, new ResultProperties(ConsoleColor.DarkRed, "× ")},
                { ResultType.Cancelled, new ResultProperties(ConsoleColor.DarkRed, "＊ ")},
                { ResultType.Unknown, new ResultProperties(ConsoleColor.DarkGray, "? ")},
            };
    }
}
