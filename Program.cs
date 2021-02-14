using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();

            var printer = new DataPrinter(1, "   • ");

            var argss = new Queue<string>(args.Select(a => a.Trim()));

            Mode mode;
            if (argss.Peek().ToLowerInvariant() == "-u")
            {
                mode = Mode.Update;
                argss.Dequeue();
            }
            else
            {
                mode = Mode.Read;
            }

            var filesData = new List<FileData?>();
            foreach (var filename in argss)
                filesData.Add(Parser.GetFileRecordOrNull(printer, filename));

            if (mode == Mode.Read)
                foreach (var fileData in filesData)
                    if (fileData == null)
                        printer.PrintError("Skipped file.");
                    else
                        printer.PrintData(fileData);
            else // (mode == Mode.Update)
                foreach (var fileData in filesData)
                    if (fileData == null)
                        printer.PrintError("Skipped file.");
                    else
                        Console.WriteLine(Updater.UpdateTags(fileData, printer));

            Console.WriteLine();
        }
    }

    public enum Mode { Read, Update }

    public static class Parser
    {
        public static FileData? GetFileRecordOrNull(DataPrinter printer, string filename)
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
