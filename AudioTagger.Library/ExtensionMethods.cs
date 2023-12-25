using System.Text;

namespace AudioTagger.Library;

public static class ExtensionMethods
{
    /// <summary>
    /// Get a friendly, human-readable version of a TimeSpan.
    /// </summary>
    public static string ElapsedFriendly(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalMilliseconds < 1000)
        {
            return $"{timeSpan.TotalMilliseconds:###0}ms";
        }

        int hours = timeSpan.Hours;
        int min = timeSpan.Minutes;
        int sec = timeSpan.Seconds;

        StringBuilder sb = new();

        if (hours > 0)
        {
            sb.Append($"{hours}h");
        }

        if (min > 0)
        {
            sb.Append(hours > 0 ? $"{min:00}m" : $"{min:0}m");
        }

        if (sec > 0)
        {
            if (hours == 0 && min == 0)
                sb.Append(timeSpan.ToString("s\\.ff") + "s");
            else if (hours > 0 || min > 0)
                sb.Append(timeSpan.ToString("ss") + "s");
            else
                sb.Append(timeSpan.ToString("s") + "s");
        }
        else
        {
            sb.Append(" exactly");
        }

        return sb.ToString();
    }
}
