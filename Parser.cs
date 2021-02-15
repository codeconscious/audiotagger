using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    public static class Parser
    {
        public static FileData? GetFileDataOrNull(DataPrinter printer, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                printer.PrintError("No filename was entered.");
                return null;
            }

            if (!File.Exists(filename))
            {
                printer.PrintError($"File \"{filename}\" was not found.");
                return null;
            }

            var taggedFile = TagLib.File.Create(filename);

            return new FileData(Path.GetFileName(filename), taggedFile);
        }
    }
}
