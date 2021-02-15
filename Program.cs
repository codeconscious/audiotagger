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
            foreach (var path in trimmedArgs)
            {
                var fileNames = PopulateFileNames(path);                

                var filesData = new List<FileData?>();
                foreach (var filename in fileNames)
                {
                    Console.WriteLine($"Found \"{filename}\"");
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
                            catch (Exception e)
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
                            catch (Exception e)
                            {
                                printer.PrintError("An known error occurred. " + e.Message);
                                continue;
                            }
                        }                            
            }            

            Console.WriteLine();
        }

        private static string[] PopulateFileNames(string fileOrDirectoryPath)
        {
            if (Directory.Exists(fileOrDirectoryPath))
                return Directory.GetFiles(fileOrDirectoryPath, "*.mp3");
            if (File.Exists(fileOrDirectoryPath))
                return new string[] { fileOrDirectoryPath };
            else
                return Array.Empty<string>();
        }
    }
}
