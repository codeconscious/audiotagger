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
    }
}