using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AudioTagger
{
    public enum UpdateableField
    {
        Artists,
        Title,
        Year,
        Genres
    }

    public class UpdateableFields
    {
        public string[]? Artists { get; set; }
        public string? Title { get; set; }
        public uint? Year { get; set; }
        public string[]? Genres { get; set; }

        public byte Count { get; }

        public UpdateableFields(IEnumerable<Group> regexElements)
        {
            foreach (var element in regexElements)
            {
                if (element.Name == "Title")
                {
                    Title = element.Value.Trim();
                    Count++;
                }
                else if (element.Name == "Artists")
                {
                    Artists = element.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    Count++;
                }
                else if (element.Name == "Year")
                {
                    Year = uint.TryParse(element.Value, out var parsed) ? parsed : 0;
                    Count++;
                }
                else if (element.Name == "Genres")
                {
                    Genres = element.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    Count++;
                }
            }
        }
    }
}