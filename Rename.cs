using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace AudioTagger
{
    public static class Renamer
    {        
        public static (bool wasDone, string message) RenameFile(FileData fileData)
        {
            Printer.Print("Entered rename method...");
            var fileName = fileData.FileNameFull;

            // Check mandatory fields
            if (string.IsNullOrWhiteSpace(fileData.Title))
                return (false, "Rename cancelled due to missing TITLE tag: " + Path.GetFileName(fileName));

            var newFileName = new StringBuilder();

            var artist = string.Join("; ", fileData.Artists);
            var title = fileData.Title;
            var year = fileData.Year.ToString(CultureInfo.InvariantCulture);
            var genre = string.Join("; ", fileData.Genres);

            if (!string.IsNullOrWhiteSpace(artist))
                newFileName.Append(artist + " - ");
            
            newFileName.Append(title);

            if (!string.IsNullOrWhiteSpace(year))
                newFileName.Append($" [{year}]");

            if (!string.IsNullOrWhiteSpace(genre))
                newFileName.Append($" {{genre}}");

            Printer.Print("Rename file:");
            Printer.Print("OLD: " + fileName);
            Printer.Print("NEW: " + newFileName.ToString());
            if (fileName.Equals(newFileName.ToString()))
                Printer.Print("(No changes)");
            
            Console.Read();

            return (false, "Testing only");
        }
    }
}