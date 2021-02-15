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
            //Console.WriteLine();

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

            foreach (var fileOrDirectoryPath in trimmedArgs)
            {
                var fileNames = PopulateFileNames(fileOrDirectoryPath);

                if (!fileNames.Any())
                {
                    Print.Error("No filenames were found for \"{path}\"...");
                    continue;
                }

                var filesData = new List<FileData?>();
                foreach (var filename in fileNames)
                {
                    //Console.WriteLine($"Found \"{filename}\"");
                    filesData.Add(Parser.GetFileDataOrNull(filename));
                }

                if (mode == Mode.Read)
                {
                    foreach (var fileData in filesData)
                    {
                        if (fileData == null)
                            Print.Error($"Skipped invalid file...");
                        else
                        {
                            try
                            {
                                Print.FileData(fileData);
                            }
                            catch (TagLib.CorruptFileException e)
                            {
                                Print.Error("The file's tag metadata was corrupt or missing." + e.Message);
                                continue;
                            }
                            catch (Exception e)
                            {
                                Print.Error("An known error occurred." + e.Message);
                                continue;
                            }
                        }
                    }
                }
                else // (mode == Mode.Update)
                {
                    foreach (var fileData in filesData)
                    {
                        if (fileData == null)
                            Print.Error("Skipped invalid file.");
                        else
                        {
                            try
                            {
                                var (updatesDone, message) = Updater.UpdateTags(fileData);
                                Print.Message(message, 0, 0, updatesDone ? "◯ " : "× ");
                            }
                            catch (TagLib.CorruptFileException e)
                            {
                                Print.Error("The file's tag metadata was corrupt or missing.  " + e.Message);
                                continue;
                            }
                            catch (Exception e)
                            {
                                Print.Error("An known error occurred. " + e.Message);
                                continue;
                            }
                        }
                    }
                }
            }            

            //Console.WriteLine();
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
