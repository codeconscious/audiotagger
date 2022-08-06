using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public class TagDuplicateFinder : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles, DirectoryInfo workingDirectory, IPrinter printer)
    {
        printer.Print("Starting...");

        // var duplicates = new Dictionary<string, List<MediaFile>>();

        // var d = mediaFiles.ToDictionary(m => string.Concat(m.Artists).Trim(), m => m);
        // var d2 = mediaFiles.ToLookup(m => string.Concat(m.Artists).Trim(), m => m).Where(m => m.Count() > 1);

        // printer.Print("Count: " + d.Count);

        // foreach (var dupe in d2)
        //     printer.Print("- " + dupe.Key);

        //foreach (var mediaFile in mediaFiles)
        //{
        //    try
        //    {
        //        isCancelled = GetTags(mediaFile);

        //        if (isCancelled)
        //            break;
        //    }
        //    catch (Exception ex)
        //    {
        //        printer.Error($"Error updating {mediaFile.FileNameOnly}: {ex.Message}");
        //        //printer.PrintException(ex);
        //        errorFiles.Add(mediaFile.FileNameOnly);
        //        continue;
        //    }
        //}

        //if (errorFiles.Any())
        //{
        //    printer.Print("Files with errors:");
        //    errorFiles.ForEach(f => printer.Print("- " + f));
        //}
    }

    // private static string GetTags(MediaFile mediaFile)
    // {

    // }
}
