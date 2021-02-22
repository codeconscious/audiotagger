using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AudioTagger
{
    public enum UpdatableField
    {
        Artists,
        Title,
        Year,
        Genres
    }

    public class UpdatableFields
    {
        public string[]? Artists { get; private set; }
        public string? Title { get; set; }
        public uint? Year { get; set; }
        public string[]? Genres { get; set; }

        public byte Count { get; }

        public UpdatableFields(IEnumerable<Group> regexElements)
        {
            foreach (var element in regexElements)
            {
                if (element.Name == "Title")
                {
                    Title = element.Value.Trim().Normalize();
                    Count++;
                }
                else if (element.Name == "Artists")
                {
                    Artists = element.Value.Split(new[] { ";" },
                                                  StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Select(a => a.Normalize())
                                           .ToArray();
                    Count++;
                }
                else if (element.Name == "Year")
                {
                    Year = uint.TryParse(element.Value, out var parsed) ? parsed : 0;
                    Count++;
                }
                else if (element.Name == "Genres")
                {
                    Genres = element.Value.Split(new[] { ";" },
                                                 StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                          .Select(g => g.Normalize())
                                          .ToArray();
                    Count++;
                }
            }
        }

        public IList<OutputLine> GetUpdateOutput(FileData fileData)
        {
            var updateOutput = new List<OutputLine>();
            var headerColor = ConsoleColor.White;
            var prependLineWith = "";

            if (Title != null && Title != fileData.Title)
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Title",
                        Title,
                        prependLineWith,
                        headerColor));

            if (Artists != null && !Artists.All(a => fileData.Artists.Contains(a)))
            {
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Artists",
                        string.Join("; ", Artists),
                        prependLineWith,
                        headerColor));
            }

            if (Year != null && Year != fileData.Year)
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Year",
                        Year.Value.ToString(CultureInfo.InvariantCulture),
                        prependLineWith,
                        headerColor));

            if (Genres != null && !Genres.All(a => fileData.Genres.Contains(a)))
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Genres",
                        string.Join("; ", Genres),
                        prependLineWith,
                        headerColor));

            return updateOutput;            
        }
    }
}