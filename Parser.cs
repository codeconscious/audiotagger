using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    public static class Parser
    {
        public static FileData? GetFileDataOrNull(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                Print.Error("No filename was entered.");
                return null;
            }

            if (!File.Exists(filename))
            {
                Print.Error($"File \"{filename}\" was not found.");
                return null;
            }

            var taggedFile = TagLib.File.Create(filename);

            return new FileData(Path.GetFileName(filename), taggedFile);
        }
    }
}
