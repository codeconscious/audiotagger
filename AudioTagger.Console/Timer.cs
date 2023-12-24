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
    private double ElapsedMs => TimeSpan.FromTicks(Stopwatch.ElapsedTicks).TotalMilliseconds;

    public OperationTimer()
    {
        Stopwatch.Start();
    }

    /// <summary>
    /// Returns a formatted version of the elapsed time since the timer was started.
    /// </summary>
    public string ElapsedTime()
    {
        // TODO: Convert to seconds, etc., as needed for larger values.
        return $"{ElapsedMs:#,##0}ms";
    }
}
