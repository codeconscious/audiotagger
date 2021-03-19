using System;

namespace AudioTagger
{
    public static class FileSelection
    {
        private static readonly StringComparison StringComparison =
            StringComparison.InvariantCultureIgnoreCase;

        public static readonly Func<string, bool> Filter =
            new(file => !string.IsNullOrWhiteSpace(file) &&
                        (file.EndsWith(".mp3", StringComparison) ||
                        file.EndsWith(".ogg", StringComparison) ||
                        file.EndsWith(".m4a", StringComparison)));    
    }
}
