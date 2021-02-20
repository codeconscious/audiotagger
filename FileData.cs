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

        public string FileName
        {
            get => _fileName;
        }

        public string Title
        {
            get => _tabLibFile.Tag.Title;
            set => _tabLibFile.Tag.Title = value.Trim();
        }

        public string[] Artists
        {
            get => _tabLibFile.Tag.Performers;
            set => _tabLibFile.Tag.Performers = value.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
        }

        public string Album
        {
            get => _tabLibFile.Tag.Album;
            set => _tabLibFile.Tag.Album = value.Trim();
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
            set => _tabLibFile.Tag.Genres = value;
        }

        public string[] Composers
        {
            get => _tabLibFile.Tag.Composers;
            set => _tabLibFile.Tag.Composers = value;
        }

        public string Comments
        {
            get => _tabLibFile.Tag.Comment ?? "";
            set => _tabLibFile.Tag.Comment = value.Trim();
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
            get => _tabLibFile.Tag.ReplayGainTrackGain > 0 ||
                   _tabLibFile.Tag.ReplayGainAlbumGain > 0;
        }

        /// <summary>
        /// Save any updated tag data to the file.
        /// </summary>
        public void SaveUpdates()
        {
            _tabLibFile.Save();
        }

        // TODO: Move to the FileData class, probably.
        public LineOutputCollection GetLineOutput()
        {
            var lines = new LineOutputCollection();

            var fileNameBase = System.IO.Path.GetFileNameWithoutExtension(FileName);
            var fileNameExt = System.IO.Path.GetExtension(FileName);
            lines.Add(
                new LineOutput(
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

            // TODO: Make labels multilingual?
            lines.Add(TagDataWithHeader("Title", Title));
            lines.Add(TagDataWithHeader("Artist(s)", string.Join(", ", Artists)));
            lines.Add(TagDataWithHeader("Album", Album));
            lines.Add(TagDataWithHeader("Year", Year.ToString()));
            lines.Add(TagDataWithHeader("Duration", Duration.ToString("m\\:ss")));
            lines.Add(TagDataWithHeader("Genre(s)", string.Join(", ", Genres)));

            var bitrate = BitRate.ToString();
            var sampleRate = SampleRate.ToString("#,##0");
            var hasReplayGain = HasReplayGainData ? "ReplayGain OK" : "No ReplayGain";
            lines.Add(TagDataWithHeader("Quality", $"{bitrate}kbps | {sampleRate}kHz | {hasReplayGain}"));
            //TagDataWithHeader(
            //    "Quality",
            //    new List<LineParts>
            //    {
            //        new LineParts(bitrate),
            //    },
            //    prependLine);

            if (Composers?.Length > 0)
                lines.Add(TagDataWithHeader($"Composers", string.Join("; ", Composers)));

            if (!string.IsNullOrWhiteSpace(Comments))
                lines.Add(TagDataWithHeader("Comment", Comments));

            return lines;

            static LineOutput TagDataWithHeader(string tagName, string tagData, string toPrepend = "")
            {
                var spacesToPrepend = 4;
                var spacesToAppend = 11 - tagName.Length; // TODO: Calcuate this instead
                var separator = ": ";

                var lineOutput = new LineOutput();

                lineOutput.Add(toPrepend);
                lineOutput.Add(new string(' ', spacesToPrepend));
                lineOutput.Add(tagName, ConsoleColor.DarkGray);
                lineOutput.Add(new string(' ', spacesToAppend));
                lineOutput.Add(separator, ConsoleColor.DarkGray);
                lineOutput.Add(tagData);

                return lineOutput;
            }
        }
    }
}
