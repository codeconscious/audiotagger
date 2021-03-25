using System;
using System.Collections.Generic;
using System.Linq;
using TagLib;

namespace AudioTagger
{
    public class FileData
    {
        public string Path { get; private set; }
        private readonly File _taggedFile; // Tag data (Rename)

        public FileData(string filePath, File tabLibFile)
        {
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException(nameof(filePath));

            Path = filePath;
            _taggedFile = tabLibFile;
        }

        public string FileNameOnly
        {
            get => System.IO.Path.GetFileName(Path);
        }

        public string Title
        {
            get => _taggedFile.Tag.Title?.Normalize() ?? "";
            set => _taggedFile.Tag.Title = value.Trim().Normalize();
        }

        public string[] Artists
        {
            get => _taggedFile.Tag.Performers?.Select(a => a?.Normalize() ?? "")
                                              .ToArray() ??
                   Array.Empty<string>();

            set => _taggedFile.Tag.Performers =
                value.Where(a => !string.IsNullOrWhiteSpace(a))
                     .Select(a => a.Trim().Normalize())
                     .ToArray();
        }

        public string Album
        {
            get => _taggedFile.Tag.Album?.Normalize() ?? "";
            set => _taggedFile.Tag.Album = value?.Trim()?.Normalize();
        }

        public uint Year
        {
            get => _taggedFile.Tag.Year;
            set => _taggedFile.Tag.Year = value;
        }

        public TimeSpan Duration
        {
            get => _taggedFile.Properties.Duration;
        }

        public string[] Genres
        {
            get => _taggedFile.Tag.Genres;
            set => _taggedFile.Tag.Genres =
                value?.Select(g => g?.Trim()?.Normalize() ?? "")?
                     .ToArray()
                ?? Array.Empty<string>();
        }

        public string[] Composers
        {
            get => _taggedFile.Tag.Composers?.Select(c => c?.Trim()?.Normalize() ?? "")?
                                             .ToArray()
                ?? Array.Empty<string>();

            set => _taggedFile.Tag.Composers =
                value?.Select(g => g?.Trim()?.Normalize())?
                      .ToArray()
                ?? Array.Empty<string>();
        }

        public string Comments
        {
            get => _taggedFile.Tag.Comment?.Trim()?.Normalize() ?? "";
            set => _taggedFile.Tag.Comment = value.Trim().Normalize();
        }

        public int BitRate
        {
            get => _taggedFile.Properties.AudioBitrate;
        }

        public int SampleRate
        {
            get => _taggedFile.Properties.AudioSampleRate;
        }

        public bool HasReplayGainData
        {
            get => _taggedFile.Tag.ReplayGainTrackGain != 0 ||
                   _taggedFile.Tag.ReplayGainAlbumGain != 0;
        }

        // The embedded image for the album, represented as an array of bytes or,
        // if none, an empty array.
        public byte[] AlbumArt
        {
           get
           {
                var albumData = _taggedFile.Tag?.Pictures;

                return albumData == null || albumData.Length == 0
                    ? Array.Empty<byte>()
                    : _taggedFile.Tag?.Pictures[0]?.Data?.Data ?? Array.Empty<byte>();
           }
        }

        /// <summary>
        /// Save any updated tag data to the file.
        /// </summary>
        public void SaveUpdates()
        {
            _taggedFile.Save();
        }

        public IList<OutputLine> GetTagPrintedLines()
        {
            var lines = new List<OutputLine>();

            var fileNameBase = System.IO.Path.GetFileNameWithoutExtension(Path);
            var fileNameExt = System.IO.Path.GetExtension(Path);
            lines.Add(
                new OutputLine(
                    new LineSubString(fileNameBase, ConsoleColor.Cyan),
                    new LineSubString(fileNameExt, ConsoleColor.DarkCyan)));

            // JA characters are wider than EN, so the alignment is off.
            // TODO: Delete if not needed.
            // Console.WriteLine(new string('—', header.Length * 2));
            // var separator = new StringBuilder();
            // foreach (var ch in header)
            // {
            //     separator.Append(ch > 256 ? '―' : '–');
            // }
            // Console.WriteLine(separator.ToString());

            lines.Add(Printer.TagDataWithHeader("Title", Title));
            lines.Add(Printer.TagDataWithHeader("Artist(s)", string.Join(", ", Artists)));
            lines.Add(Printer.TagDataWithHeader("Album", Album));
            lines.Add(Printer.TagDataWithHeader("Year", Year.ToString()));
            lines.Add(Printer.TagDataWithHeader("Duration", Duration.ToString("m\\:ss")));

            var genreCount = Genres.Length;
            lines.Add(Printer.TagDataWithHeader("Genre(s)", string.Join(", ", Genres) +
                                                (genreCount > 1 ? $" ({genreCount})" : "")));

            var bitrate = BitRate.ToString();
            var sampleRate = SampleRate.ToString("#,##0");
            var hasReplayGain = HasReplayGainData ? "ReplayGain OK" : "No ReplayGain";

            // Create formatted quality line            
            const string genreSeparator = "    ";
            lines.Add(Printer.TagDataWithHeader(
                "Quality",
                new List<LineSubString>
                {
                    new LineSubString(bitrate),
                    new LineSubString(" kbps" + genreSeparator, ConsoleColor.DarkGray),
                    new LineSubString(sampleRate),
                    new LineSubString(" kHz" + genreSeparator, ConsoleColor.DarkGray),
                    new LineSubString(hasReplayGain)
                }));

            if (Composers?.Length > 0)
                lines.Add(Printer.TagDataWithHeader($"Composers", string.Join("; ", Composers)));

            if (!string.IsNullOrWhiteSpace(Comments))
                lines.Add(Printer.TagDataWithHeader("Comment", Comments));

            return lines;            
        }
    }
}
