using System;

namespace AudioTagger
{
    public static class FileSelection
    {
        private static StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase;

        public static readonly Func<string, bool> Filter =
            new(
                file =>
                    file.EndsWith(".mp3", StringComparison) ||
                    file.EndsWith(".ogg", StringComparison) ||
                    file.EndsWith(".m4a", StringComparison));    
    }
}
