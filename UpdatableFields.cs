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
        Album,
        Year,
        Genres
    }

    public class UpdatableFields
    {
        public string[]? Artists { get; private set; }
        public string? Title { get; set; }
        public string? Album { get; set; }
        public uint? Year { get; set; }
        public string[]? Genres { get; set; }

        public byte Count { get; }

        public UpdatableFields(IEnumerable<Group> regexElements)
        {
            foreach (var element in regexElements)
            {
                if (element.Name == "title")
                {
                    Title = element.Value.Trim().Normalize();
                    Count++;
                }
                else if (element.Name == "artists")
                {
                    Artists = element.Value.Split(new[] { ";" },
                                                  StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Select(a => a.Normalize())
                                           .ToArray();
                    Count++;
                }
                else if (element.Name == "album")
                {
                    Album = element.Value.Trim().Normalize();
                    Count++;
                }
                else if (element.Name == "genres")
                {
                    Genres = element.Value.Split(new[] { ";" },
                                                 StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                          .Select(g => g.Normalize())
                                          .ToArray();
                    Count++;
                }
                else if (element.Name == "year")
                {
                    Year = uint.TryParse(element.Value, out var parsed) ? parsed : 0;
                    Count++;
                }
            }
        }

        public IList<OutputLine> GetUpdateOutput(FileData fileData)
        {
            var updateOutput = new List<OutputLine>();
            var headerColor = ConsoleColor.White;
            var prependLineWith = "";

            if (Artists != null && !Artists.All(a => fileData.Artists.Contains(a)))
            {
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Artists",
                        string.Join("; ", Artists),
                        prependLineWith,
                        headerColor));
            }

            if (Title != null && Title != fileData.Title)
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Title",
                        Title,
                        prependLineWith,
                        headerColor));

            if (Album != null && Album != fileData.Album)
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Album",
                        Album,
                        prependLineWith,
                        headerColor));

            if (Year != null && Year != fileData.Year)
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Year",
                        Year.Value.ToString(CultureInfo.InvariantCulture),
                        prependLineWith,
                        headerColor));

            if (Genres != null && !Genres.All(a => fileData.Genres.Contains(a)))
            {
                var genreCount = Genres.Length;
                updateOutput.Add(
                    Printer.TagDataWithHeader(
                        "Genres",
                        string.Join("; ", Genres) + (genreCount > 1 ? $" ({genreCount})" : ""),
                        prependLineWith,
                        headerColor));
            }

            return updateOutput;            
        }
    }
}