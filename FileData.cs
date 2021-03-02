using System;
using System.Collections.Generic;
using System.Linq;
using TagLib;

namespace AudioTagger
{
    public class FileData
    {
        private string _fileName;
        private File _tabLibFile;

        public FileData(string fileName, File tabLibFile)
        {
            _fileName = fileName;
            _tabLibFile = tabLibFile;
        }

        public string FileNameFull
        {
            get => _fileName;
        }

        public string FileNameShort
        {
            get => System.IO.Path.GetFileName(_fileName);
        }

        public string Title
        {
            get => _tabLibFile.Tag.Title?.Normalize() ?? "";
            set => _tabLibFile.Tag.Title = value.Trim().Normalize();
        }

        public string[] Artists
        {
            get => _tabLibFile.Tag.Performers?.Select(a => a?.Normalize() ?? "")
                                              .ToArray() ??
                   Array.Empty<string>();

            set => _tabLibFile.Tag.Performers =
                value.Where(a => !string.IsNullOrWhiteSpace(a))
                     .Select(a => a.Trim().Normalize())
                     .ToArray();
        }

        public string Album
        {
            get => _tabLibFile.Tag.Album?.Normalize() ?? "";
            set => _tabLibFile.Tag.Album = value?.Trim()?.Normalize();
        }

        public uint Year
        {
            get => _tabLibFile.Tag.Year;
            set => _tabLibFile.Tag.Year = value;
        }

        public TimeSpan Duration
        {
            get => _tabLibFile.Properties.Duration;
        }

        public string[] Genres
        {
            get => _tabLibFile.Tag.Genres;
            set => _tabLibFile.Tag.Genres =
                value?.Select(g => g?.Trim()?.Normalize() ?? "")?
                     .ToArray()
                ?? Array.Empty<string>();
        }

        public string[] Composers
        {
            get => _tabLibFile.Tag.Composers?.Select(c => c?.Trim()?.Normalize() ?? "")?
                                             .ToArray()
                ?? Array.Empty<string>();

            set => _tabLibFile.Tag.Composers =
                value?.Select(g => g?.Trim()?.Normalize())?
                      .ToArray()
                ?? Array.Empty<string>();
        }

        public string Comments
        {
            get => _tabLibFile.Tag.Comment?.Trim()?.Normalize() ?? "";
            set => _tabLibFile.Tag.Comment = value.Trim().Normalize();
        }

        public int BitRate
        {
            get => _tabLibFile.Properties.AudioBitrate;
        }

        public int SampleRate
        {
            get => _tabLibFile.Properties.AudioSampleRate;
        }

        public bool HasReplayGainData
        {
            get => _tabLibFile.Tag.ReplayGainTrackGain != 0 ||
                   _tabLibFile.Tag.ReplayGainAlbumGain != 0;
        }

        /// <summary>
        /// Save any updated tag data to the file.
        /// </summary>
        public void SaveUpdates()
        {
            _tabLibFile.Save();
        }

        public IList<OutputLine> GetTagOutput()
        {
            var lines = new List<OutputLine>();

            var fileNameBase = System.IO.Path.GetFileNameWithoutExtension(FileNameFull);
            var fileNameExt = System.IO.Path.GetExtension(FileNameFull);
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
