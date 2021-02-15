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

            var trimmedArgs = new Queue<string>(args.Select(a => a.Trim()));

            // Set the action mode
            Mode mode;
            if (trimmedArgs.Peek().ToLowerInvariant() == "-u")
            {
                mode = Mode.Update;
                trimmedArgs.Dequeue();
            }
            else
            {
                mode = Mode.Read;
            }

            // Iterate over each arg, checking if it's a file or directory
            var fileNames = new List<string>();
            foreach (var path in trimmedArgs)
            {
                if (Directory.Exists(path))
                    fileNames.AddRange(Directory.GetFiles(path, "*.mp3"));
                else if (File.Exists(path))
                    fileNames.Add(path);
                else
                    printer.PrintError("Invalid item: " + path);

                var filesData = new List<FileData?>();
                foreach (var filename in fileNames)
                {
                    Console.WriteLine("Working on " + filename);
                    filesData.Add(Parser.GetFileRecordOrNull(printer, filename));
                }

                if (mode == Mode.Read)
                    foreach (var fileData in filesData)
                        if (fileData == null)
                            printer.PrintError("Skipped file.");
                        else
                        {
                            try
                            {
                                printer.PrintData(fileData);
                            }
                            catch (TagLib.CorruptFileException e)
                            {
                                printer.PrintError("The file's tag metadata was corrupt or missing." + e.Message);
                                continue;
                            }
                            catch (System.Exception e)
                            {
                                printer.PrintError("An known error occurred." + e.Message);
                                continue;
                            }                            
                        }
                else // (mode == Mode.Update)
                    foreach (var fileData in filesData)
                        if (fileData == null)
                            printer.PrintError("Skipped invalid file.");
                        else
                        {
                            try
                            {
                                Console.WriteLine(Updater.UpdateTags(fileData, printer));
                            }
                            catch (TagLib.CorruptFileException e)
                            {
                                printer.PrintError("The file's tag metadata was corrupt or missing.  " + e.Message);
                                continue;
                            }
                            catch (System.Exception e)
                            {
                                printer.PrintError("An known error occurred. " + e.Message);
                                continue;
                            }
                        }                            
            }            

            Console.WriteLine();
        }
    }
}
