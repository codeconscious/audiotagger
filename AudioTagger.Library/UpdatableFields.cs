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
        public string[]? Artists { get; }
        public string? Title { get; }
        public string? Album { get; }
        public uint? Year { get; }
        public string[]? Genres { get; }

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

        public IList<OutputLine> GetUpdateOutput(MediaFile fileData, IPrinter printer)
        {
            var updateOutput = new List<OutputLine>();
            const ConsoleColor headerColor = ConsoleColor.White;
            const string prependLineWith = "";

            if (Artists?.All(a => fileData.Artists.Contains(a)) == false)
            {
                updateOutput.Add(
                    printer.TagDataWithHeader(
                        "Artists",
                        string.Join("; ", Artists),
                        prependLineWith,
                        headerColor));
            }

            if (Title != null && Title != fileData.Title)
            {
                updateOutput.Add(
                    printer.TagDataWithHeader(
                        "Title",
                        Title,
                        prependLineWith,
                        headerColor));
            }

            if (Album != null && Album != fileData.Album)
            {
                updateOutput.Add(
                    printer.TagDataWithHeader(
                        "Album",
                        Album,
                        prependLineWith,
                        headerColor));
            }

            if (Year != null && Year != fileData.Year)
            {
                updateOutput.Add(
                    printer.TagDataWithHeader(
                        "Year",
                        Year.Value.ToString(CultureInfo.InvariantCulture),
                        prependLineWith,
                        headerColor));
            }

            if (Genres?.All(a => fileData.Genres.Contains(a)) == false)
            {
                var genreCount = Genres.Length;
                updateOutput.Add(
                    printer.TagDataWithHeader(
                        "Genres",
                        string.Join("; ", Genres) + (genreCount > 1 ? $" ({genreCount})" : ""),
                        prependLineWith,
                        headerColor));
            }

            return updateOutput;
        }
    }
}