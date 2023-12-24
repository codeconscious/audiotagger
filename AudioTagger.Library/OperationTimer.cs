using System.Diagnostics;

namespace AudioTagger.Console;

/// <summary>
/// A `Stopwatch` wrapper. Starts the enclosed `Stopwatch` upon instantiation.
/// </summary>
public sealed class OperationTimer
{
    private readonly Stopwatch Stopwatch = new();

    /// <summary>
    /// The elapsed milliseconds since the stopwatch was started.
    /// </summary>
    /// <remarks>
    /// Using ticks because .ElapsedMilliseconds can be wildly inaccurate.
    /// Reference: https://stackoverflow.com/q/5113750/11767771
    /// </remarks>
    private double ElapsedMs => TimeSpan.FromTicks(Stopwatch.Elapsed.Ticks).TotalMilliseconds;

    public OperationTimer()
    {
        Stopwatch.Start();
    }

    /// <summary>
    /// Returns a formatted version of the elapsed time since the timer was started.
    /// </summary>
    public string ElapsedTime()
    {
        // return Utilities.FormatMsAsTime(ElapsedMs) + $"({ElapsedMs}ms)";

        var formatString = GetFormatString(ElapsedMs);
        // return string.Format("Time elapsed: {0:hh\\:mm\\:ss}", TimeSpan.FromTicks(Stopwatch.Elapsed.Ticks));
        return TimeSpan.FromTicks(Stopwatch.Elapsed.Ticks).ToString(formatString.Item1) + formatString.Item2;
    }

    private static (string, string) GetFormatString(double milliseconds)
    {
        return milliseconds switch
        {
            > 3_600_000 => ("hh\\:mm\\:ss", string.Empty), // >= 1 hour
            > 60_000 => ("mm\\:ss", string.Empty), // >= 1 minute
            > 1_000 => ("ss\\:ff", string.Empty), // 1 second
            _ => ("ss\\:fff", "ms"),
            // _ => "",
        };
    }
}
