using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AudioTagger
{
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

        public IList<OutputLine> GetUpdateOutput(MediaFile fileData)
        {
            var updateOutput = new List<OutputLine>();
            const string prependLineWith = "";

            if (Artists?.All(a => fileData.Artists.Contains(a)) == false)
            {
                updateOutput.Add(
                    OutputLine.TagDataWithHeader(
                        "Artists",
                        string.Join("; ", Artists),
                        prependLineWith));
            }

            if (Title != null && Title != fileData.Title)
            {
                updateOutput.Add(
                    OutputLine.TagDataWithHeader(
                        "Title",
                        Title,
                        prependLineWith));
            }

            if (Album != null && Album != fileData.Album)
            {
                updateOutput.Add(
                    OutputLine.TagDataWithHeader(
                        "Album",
                        Album,
                        prependLineWith));
            }

            if (Year != null && Year != fileData.Year)
            {
                updateOutput.Add(
                    OutputLine.TagDataWithHeader(
                        "Year",
                        Year.Value.ToString(CultureInfo.InvariantCulture),
                        prependLineWith));
            }

            if (Genres?.All(a => fileData.Genres.Contains(a)) == false)
            {
                var genreCount = Genres.Length;

                updateOutput.Add(
                    OutputLine.TagDataWithHeader(
                        "Genres",
                        string.Join("; ", Genres) + (genreCount > 1 ? $" ({genreCount})" : ""),
                        prependLineWith));
            }

            return updateOutput;
        }

        public Dictionary<string, string> GetUpdateKeyValuePairs(MediaFile fileData)
        {
            var updateOutput = new Dictionary<string, string>();

            if (Artists?.All(a => fileData.Artists.Contains(a)) == false)
            {
                updateOutput.Add("Artists", string.Join("; ", Artists));
            }

            if (Title != null && Title != fileData.Title)
            {
                updateOutput.Add("Title", Title);
            }

            if (Album != null && Album != fileData.Album)
            {
                updateOutput.Add("Album", Album);
            }

            if (Year != null && Year != fileData.Year)
            {
                updateOutput.Add("Year", Year.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (Genres?.All(a => fileData.Genres.Contains(a)) == false)
            {
                var genreCount = Genres.Length;

                updateOutput.Add(
                    "Genres",
                    string.Join("; ", Genres) + (genreCount > 1 ? $" ({genreCount})" : ""));
            }

            return updateOutput;
        }
    }
}