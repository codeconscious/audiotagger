using System.Diagnostics;
using AudioTagger.Library;

namespace AudioTagger.Console;

/// <summary>
/// A `Stopwatch` wrapper. Starts the enclosed `Stopwatch` upon instantiation.
/// </summary>
public sealed class Timer
{
    private readonly Stopwatch Stopwatch = new();

    /// <summary>
    /// The elapsed milliseconds since the stopwatch was started.
    /// </summary>
    /// <remarks>
    /// Using ticks because .ElapsedMilliseconds can be wildly inaccurate.
    /// Reference: https://stackoverflow.com/q/5113750/11767771
    /// </remarks>
    private TimeSpan ElapsedTimeSpan => TimeSpan.FromTicks(Stopwatch.Elapsed.Ticks);

    public Timer()
    {
        Stopwatch.Start();
    }

    /// <summary>
    /// Returns a formatted version of the elapsed time since the timer was started.
    /// </summary>
    public string ElapsedTimeFriendly()
    {
        return ElapsedTimeSpan.ElapsedFriendly();
    }
}
