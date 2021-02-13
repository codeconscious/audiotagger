﻿using System;

namespace AudioTagger
{
    public record FileData(
        string Title,
        string[] Artists,
        TimeSpan Duration,
        string[] Genres,
        int BitRate,
        int SampleRate,
        string[] Composers
    );
}
