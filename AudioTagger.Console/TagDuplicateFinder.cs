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
        printer.Print("Checking for duplicates (by artist(s) and title)...");

        var duplicates = mediaFiles
            .ToLookup(m => string.Concat(m.Artists).Trim() + m.Title.Trim())
            .Where(m => m.Count() > 1)
            .OrderBy(m => m.Key);

        printer.Print(duplicates.Count() + " found.");

        foreach (var dupe in duplicates)
        {
            var artist = string.Concat(dupe.First().Artists);
            var title = dupe.First().Title;
            printer.Print($"  - {artist} /  {title}");
        }
    }
}
