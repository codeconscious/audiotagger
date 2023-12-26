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
    /// Returns a formatted version of the elapsed time since the timer was started.
    /// </summary>
    /// <remarks>
    /// Using ticks because .ElapsedMilliseconds can be wildly inaccurate.
    /// (Reference: https://stackoverflow.com/q/5113750/11767771)
    /// Also, use `Stopwatch.Elapsed.Ticks` over `Stopwatch.ElapsedTicks`.
    /// For some reason, the latter returns unexpected values.
    /// </remarks>
    public string ElapsedFriendly =>
        TimeSpan.FromTicks(Stopwatch.Elapsed.Ticks)
                .ElapsedFriendly();

    public Timer()
    {
        Stopwatch.Start();
    }
}
