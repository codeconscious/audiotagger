namespace AudioTagger;

public static partial class Utilities
{
    public static string FormatMsAsTime(double milliseconds)
    {
        TimeUnit unit = GetTimeUnit(milliseconds);
        return unit switch
        {
            TimeUnit.Hours => $"{(milliseconds/3_600_000):#,##0.##}hr",
            TimeUnit.Minutes => $"{(milliseconds/60_000):#,##0.##}min",
            TimeUnit.Seconds => $"{(milliseconds/1_000):#,##0.##}s",
            _ => $"{milliseconds:#,##0}ms"
        };
    }

    private static TimeUnit GetTimeUnit(double milliseconds)
    {
        return milliseconds switch
        {
            > 7_200_000 => TimeUnit.Hours, // >= 2 hours
            > 120_000 => TimeUnit.Minutes, // >= 2 minutes
            > 1_000 => TimeUnit.Seconds, // 1 second
            _ => TimeUnit.Milliseconds
        };
    }

    private enum TimeUnit
    {
        Milliseconds,
        Seconds,
        Minutes,
        Hours
    }
}
