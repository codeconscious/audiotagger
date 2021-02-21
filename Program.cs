using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace AudioTagger
{
    static class Program
    {
        private static Func<string, bool> fileFilter =
            new Func<string, bool>(
                file =>
                    file.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase) ||
                    file.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase) ||
                    file.EndsWith(".m4a", StringComparison.InvariantCultureIgnoreCase));

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Printer.Print("Audio tagger and renamer");
                Printer.Print("Usage: jaudiotag [COMMAND] [FILES/DIRECTORIES]...", 0, 0); // TODO: Decide on a name
                Printer.Print("Supply one command, followed by one or more files or directories to process.", 0, 1);
                Printer.Print("Commands:");
                Printer.Print("  -v: View tags (default, optional)");
                Printer.Print("  -u: Update tags");
                Printer.Print("  -r: Rename files based on their tags (Coming soonish)", 0, 1);
                // TODO: Add option to disable colors
                
                return;
            }

            var trimmedArgs = new Queue<string>(args.Select(a => a.Trim()));

            // Set the action mode
            Mode mode;
            if (trimmedArgs.Peek().ToLowerInvariant() == "-u")
            {
                mode = Mode.Update;
                trimmedArgs.Dequeue();
            }
            else if (trimmedArgs.Peek().ToLowerInvariant() == "-r")
            {
                mode = Mode.Rename;
                trimmedArgs.Dequeue();
            }
            else
            {
                mode = Mode.View;
            }

            foreach (var fileOrDirectoryPath in trimmedArgs)
            {
                var fileNames = PopulateFileNames(fileOrDirectoryPath);

                if (!fileNames.Any())
                {
                    Printer.Error($"No filenames were found for \"{fileOrDirectoryPath}\"...");
                    continue;
                }

                var filesData = new List<FileData?>();
                foreach (var filename in fileNames)
                {
                    //Console.WriteLine($"Found \"{filename}\"");
                    filesData.Add(Parser.GetFileDataOrNull(filename));
                }

                //filesData.Sort();

                if (mode == Mode.View)
                {
                    foreach (var fileData in filesData)
                    {
                        if (fileData == null)
                            Printer.Error($"Skipped invalid file..."); // TODO: Refactor identical checks
                        else
                        {
                            try
                            {
                                //Printer.FileData(fileData, "", 0, 1);
                                Printer.Print(fileData.GetTagsAsOutputLines());
                            }
                            catch (TagLib.CorruptFileException e)
                            {
                                Printer.Error("The file's tag metadata was corrupt or missing." + e.Message);
                                continue;
                            }
                            catch (Exception e)
                            {
                                Printer.Error("An unknown error occurred." + e.Message);
                                continue;
                            }
                        }
                    }
                }
                else if (mode == Mode.Rename)
                {
                    foreach (var fileData in filesData)
                    {
                        if (fileData == null)
                            Printer.Error($"Skipped invalid file...");
                        else
                        {
                            try
                            {
                                var (wasDone, message) = Renamer.RenameFile(fileData);
                                Printer.Print(wasDone ? "◯ " : "× " + message);
                            }
                            catch (TagLib.CorruptFileException e)
                            {
                                Printer.Error("The file's tag metadata was corrupt or missing." + e.Message);
                                continue;
                            }
                            catch (Exception e)
                            {
                                Printer.Error("An error occurred:" + e.Message);
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
                            Printer.Error("Skipped invalid file.");
                        else
                        {
                            try
                            {
                                var (updatesDone, message, cancel) = Updater.UpdateTags(fileData);
                                Printer.Print(message, 0, updatesDone ? 1 : 0, updatesDone ? "◯ " : "× ");
                                if (cancel)
                                    break;
                            }
                            catch (TagLib.CorruptFileException e)
                            {
                                Printer.Error("The file's tag metadata was corrupt or missing.  " + e.Message);
                                continue;
                            }
                            catch (Exception e)
                            {
                                Printer.Error("An error occurred: " + e.Message);
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
            {
                return Directory
                    .EnumerateFiles(fileOrDirectoryPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(fileFilter)
                    .ToArray();
            }
            if (File.Exists(fileOrDirectoryPath))
                return new string[] { fileOrDirectoryPath };

            else
                return Array.Empty<string>();
        }
    }
}
