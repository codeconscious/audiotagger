using System;
using System.Collections.Generic;
using System.Linq;
using TagLib;

namespace AudioTagger
{
    public class FileData
    {
        public string Path { get; }
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
                                              .ToArray()
                    ?? Array.Empty<string>();

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

            set => _taggedFile.Tag.Composers = value?.Select(g => g?.Trim()?
                                                                    .Normalize())?
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

        /// <summary>
        /// Get a list of FileData objects.
        /// </summary>
        /// <param name="path">A directory or file path</param>
        /// <returns></returns>
        public static IReadOnlyCollection<FileData> PopulateFileData(string path)
        {
            if (System.IO.Directory.Exists(path)) // i.e., the path is a directory
            {
                var filesData = new List<FileData>();

                var fileNames = System.IO.Directory.EnumerateFiles(path,
                                                                   "*.*",
                                                                   System.IO.SearchOption.TopDirectoryOnly) // TODO: Make option
                                                   .Where(FileSelection.Filter)
                                                   .ToArray();

                foreach (var fileName in fileNames)
                {
                    filesData.Add(Parser.CreateFileData(fileName));
                }

                return filesData/*.OrderBy(f => f.Artists)
                                .ThenBy(f => f.Title)
                                .AsEnumerable()
                                .ToList()*/;
            }

            if (System.IO.File.Exists(path)) // i.e., the path is a file
            {
                return new List<FileData> { Parser.CreateFileData(path) };
            }

            throw new InvalidOperationException($"The path \"{path}\" was invalid.");
        }
    }
}
