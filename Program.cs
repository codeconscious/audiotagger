﻿using System;
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
            if (args.Length == 0)
            {
                PrintInstructions();
                return;
            }

            var trimmedArgs = new Queue<string>(args.Select(a => a.Trim()));

            IPathProcessor? processor = ProcessorFactory(trimmedArgs.Dequeue());

            if (processor == null)
            {
                PrintInstructions();
                return;
            }

            foreach (var path in trimmedArgs)
            {
                IReadOnlyCollection<FileData> filesData;
                try
                {
                    filesData = PopulateFileData(path);
                }
                catch (InvalidOperationException ex)
                {
                    Printer.Error($"Path \"{path}\" could not be fully parsed. ERROR: " + ex.Message);
                    continue;
                }                

                if (!filesData.Any())
                {
                    Printer.Error($"No files found at \"{path}\".");
                    continue;
                }

                processor.Start(filesData);
            }
        }

        private static IPathProcessor? ProcessorFactory(string modeArg)
        {
            return modeArg.ToLowerInvariant() switch
            {
                "-u" or "--update" => new TagUpdater(),
                "-r" or "--rename" => new FileRenamer(),
                "-v" or "--view" => new TagViewer(),
                _ => null
            };
        }

        /// <summary>
        /// Get a list of FileData objects.
        /// </summary>
        /// <param name="path">A directory or file path</param>
        /// <returns></returns>
        private static List<FileData> PopulateFileData(string path)
        {
            if (Directory.Exists(path))
            {
                var filesData = new List<FileData>();

                var fileNames = Directory.EnumerateFiles(path,
                                                         "*.*",
                                                         SearchOption.TopDirectoryOnly) // TODO: Make option
                                         .Where(FileSelection.Filter)
                                         .ToArray();

                foreach (var fileName in fileNames)
                {
                    filesData.Add(Parser.GetFileData(fileName));
                }

                //filesData.Sort(); // Ref: https://stackoverflow.com/a/1658749/11767771

                return filesData;
            }

            if (File.Exists(path))
            {
                return new List<FileData> { Parser.GetFileData(path) };
            }

            throw new InvalidOperationException($"The path \"{path}\" was invalid.");
        }

        private static void PrintInstructions()
        {
            Printer.Print("Audio tagger and (eventually) renamer");
            Printer.Print("Usage: jaudiotag [COMMAND] [FILES/DIRECTORIES]...", 0, 1, ""); // TODO: Decide on a name
            Printer.Print("Supply one command, followed by one or more files or directories to process.", 0, 1, "");
            Printer.Print("Commands:");
            Printer.Print("   -v or --view   : View tags");
            Printer.Print("   -u or --update : Update tags");
            Printer.Print("   -r or --rename : Rename files based on tags (Coming soonish)");
            // TODO: Add option to disable colors
        }
    }
}