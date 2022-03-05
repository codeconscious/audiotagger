using TagLib;

namespace AudioTagger
{
    public class MediaFile
    {
        public string Path { get; }
        private readonly TagLib.File _taggedFile; // Tag data (Rename)

        public MediaFile(string filePath, TagLib.File tabLibFile)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            ArgumentNullException.ThrowIfNull(tabLibFile);

            Path = filePath;
            _taggedFile = tabLibFile;
        }

        public string FileNameOnly
        {
            get => System.IO.Path.GetFileName(Path);
        }

        public string Title
        {
            get => _taggedFile.Tag.Title?.Normalize() ?? string.Empty;
            set => _taggedFile.Tag.Title = value.Trim().Normalize();
        }

        public string[] Artists
        {
            get => _taggedFile.Tag.Performers?.Select(a => a?.Normalize() ?? string.Empty)
                                              .ToArray()
                    ?? Array.Empty<string>();

            set => _taggedFile.Tag.Performers =
                        value.Where(a => !string.IsNullOrWhiteSpace(a))
                            .Select(a => a.Trim().Normalize())
                            .ToArray();
        }

        public string Album
        {
            get => _taggedFile.Tag.Album?.Normalize() ?? string.Empty;
            set => _taggedFile.Tag.Album = value?.Trim()?.Normalize();
        }

        public uint Year
        {
            get => _taggedFile.Tag.Year;
            set => _taggedFile.Tag.Year = value;
        }

        public uint TrackNo
        {
            get => _taggedFile.Tag.Track;
            set => _taggedFile.Tag.Track = value;
        }

        public TimeSpan Duration
        {
            get => _taggedFile.Properties.Duration;
        }

        public string[] Genres
        {
            get => _taggedFile.Tag.Genres;
            set => _taggedFile.Tag.Genres =
                        value?.Select(g => g?.Trim()?.Normalize() ?? string.Empty)?
                            .ToArray()
                        ?? Array.Empty<string>();
        }

        public string[] Composers
        {
            get => _taggedFile.Tag.Composers?.Select(c => c?.Trim()?.Normalize() ?? string.Empty)?
                                             .ToArray()
                    ?? Array.Empty<string>();

            set => _taggedFile.Tag.Composers = value?.Select(g => g?.Trim()?
                                                                    .Normalize())?
                                                     .ToArray()
                                               ?? Array.Empty<string>();
        }

        public string Comments
        {
            get => _taggedFile.Tag.Comment?.Trim()?.Normalize() ?? string.Empty;
            set => _taggedFile.Tag.Comment = value.Trim().Normalize();
        }

        public string Lyrics
        {
            get => _taggedFile.Tag.Lyrics?.Trim()?.Normalize() ?? string.Empty;
            set => _taggedFile.Tag.Lyrics = value.Trim().Normalize();
        }

        public int BitRate
        {
            get => _taggedFile.Properties.AudioBitrate;
        }

        public int SampleRate
        {
            get => _taggedFile.Properties.AudioSampleRate;
        }

        // TODO: Verify this is working correctly.
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
                    : _taggedFile.Tag?.Pictures[0]?.Data?.Data
                      ?? Array.Empty<byte>();
           }
        }

        /// <summary>
        /// Save updated tag data to the file.
        /// </summary>
        public void SaveUpdates()
        {
            _taggedFile.Save();
        }

        /// <summary>
        /// Given a folder or file path, returns a list of AudioFile for each file.
        /// Thus, a path to a file will always return a collection of one item,
        /// and a path to a folder will return an AudioFile for each file within that folder.
        /// </summary>
        /// <param name="path">A directory or file path</param>
        /// <returns>A collection of MediaFile</returns>
        public static IReadOnlyCollection<MediaFile> PopulateFileData(string path, bool searchSubDirectories = false)
        {
            // If the path is a directory
            if (System.IO.Directory.Exists(path))
            {
                var mediaFiles = new List<MediaFile>();

                var fileNames = System.IO.Directory.EnumerateFiles(path,
                                                                   "*.*",
                                                                   searchSubDirectories
                                                                        ? System.IO.SearchOption.AllDirectories
                                                                        : System.IO.SearchOption.TopDirectoryOnly)
                                                   .Where(FileSelection.Filter)
                                                   .ToArray();

                foreach (var fileName in fileNames)
                {
                    mediaFiles.Add(MediaFileFactory.CreateFileData(fileName));
                }

                return mediaFiles/*.OrderBy(f => f.Artists)
                                .ThenBy(f => f.Title)
                                .AsEnumerable()
                                .ToList()*/;
            }

            // If the path is a file
            if (System.IO.File.Exists(path))
            {
                return new List<MediaFile> { MediaFileFactory.CreateFileData(path) };
            }

            throw new InvalidOperationException($"The path \"{path}\" was invalid.");
        }
    }
}
