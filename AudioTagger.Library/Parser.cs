using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    public static class Parser
    {
        public static AudioFile CreateFileData(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(nameof(filePath));
            }

            var taggedFile = TagLib.File.Create(filePath);

            return new AudioFile(filePath, taggedFile);
        }
    }
}
