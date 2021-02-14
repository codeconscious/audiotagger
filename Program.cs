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

            var printer = new DataPrinter(1, "  • ");

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

            var fileNames = new List<string>();
            foreach (var path in argss)
            {
                if (Directory.Exists(path))
                    fileNames.AddRange(Directory.GetFiles(path, "*.mp3"));
                else if (File.Exists(path))
                    fileNames.Add(path);
                else
                    printer.PrintError("Could not determine if directory or file: " + path);

                var filesData = new List<FileData?>();
                foreach (var filename in fileNames)
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
            }            

            Console.WriteLine();
        }
    }
}
