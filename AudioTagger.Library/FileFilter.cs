using System;

namespace AudioTagger
{
    public static class FileSelection
    {
        public static readonly Func<string, bool> Filter =
            new(file => !string.IsNullOrWhiteSpace(file) &&
                        (file.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase) ||
                         file.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase) ||
                         file.EndsWith(".m4a", StringComparison.InvariantCultureIgnoreCase)));
    }
}
