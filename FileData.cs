using System;

namespace AudioTagger
{
    public record FileData(
        string FileName,
        string Title,
        string[] Artists,
        string Album,
        uint Year,
        TimeSpan Duration,
        string[] Genres,
        int BitRate,
        int SampleRate,
        string[] Composers,
        bool HasReplayGainData
    );
}
