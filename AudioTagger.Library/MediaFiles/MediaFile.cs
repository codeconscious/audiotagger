namespace AudioTagger.Library.MediaFiles;

public sealed class MediaFile
{
    public string Path { get; }
    private readonly TagLib.File _taggedFile;

    public MediaFile(string filePath, TagLib.File tabLibFile)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(tabLibFile);

        Path = filePath;
        _taggedFile = tabLibFile;
    }

    public string FileNameOnly => System.IO.Path.GetFileName(Path);

    public long FileSizeInBytes => new System.IO.FileInfo(Path).Length;

    public string Title
    {
        get => _taggedFile.Tag.Title?.Normalize() ?? string.Empty;
        set => _taggedFile.Tag.Title = value.Trim().Normalize();
    }

    public string[] AlbumArtists
    {
        get => _taggedFile.Tag.AlbumArtists?.Select(a => a?.Normalize() ?? string.Empty)
                                            .ToArray()
                ?? Array.Empty<string>();

        set => _taggedFile.Tag.AlbumArtists =
                    value.Where(a => !string.IsNullOrWhiteSpace(a))
                         .Select(a => a.Trim().Normalize())
                         .Distinct()
                         .ToArray();
    }

    // TODO: Note why Performers is used instead of Artists.
    public string[] Artists
    {
        get => _taggedFile.Tag.Performers?.Select(a => a?.Normalize() ?? string.Empty)
                                          .ToArray()
                ?? Array.Empty<string>();

        set => _taggedFile.Tag.Performers =
                    value.Where(a => !string.IsNullOrWhiteSpace(a))
                         .Select(a => a.Trim().Normalize())
                         .Distinct()
                         .ToArray();
    }

    public string Album
    {
        get => _taggedFile.Tag.Album?.Normalize() ?? string.Empty;
        set => _taggedFile.Tag.Album = value?.Trim()?.Normalize();
    }

    public bool LacksArtists => !AlbumArtists.Any() && !Artists.Any();

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

        // TODO: Add first genre tag too?
        set => _taggedFile.Tag.Genres = value?.Select(g => g?.Trim()?.Normalize()
                                                      ?? string.Empty)?
                                              .ToArray()
                                        ?? Array.Empty<string>();
    }

    public string[] Composers
    {
        get => _taggedFile.Tag.Composers?.Select(c => c?.Trim()?.Normalize()
                                                      ?? string.Empty)?
                                         .ToArray()
               ?? Array.Empty<string>();

        set => _taggedFile.Tag.Composers = value?.Select(g => g?.Trim()?
                                                                .Normalize())?
                                                 .ToArray()
                                           ?? Array.Empty<string>();
    }

    public string Lyrics
    {
        get => _taggedFile.Tag.Lyrics?.Trim()?.Normalize() ?? string.Empty;
        set => _taggedFile.Tag.Lyrics = value.Trim().Normalize();
    }

    public string Comments
    {
        get => _taggedFile.Tag.Comment?.Trim()?.Normalize() ?? string.Empty;
        set => _taggedFile.Tag.Comment = value.Trim().Normalize();
    }

    public string Description
    {
        get => _taggedFile.Tag.Description?.Trim()?.Normalize() ?? string.Empty;
        set => _taggedFile.Tag.Description = value.Trim().Normalize();
    }

    public int BitRate => _taggedFile.Properties.AudioBitrate;

    public int SampleRate => _taggedFile.Properties.AudioSampleRate;

    public double ReplayGainTrack => _taggedFile.Tag.ReplayGainTrackGain;

    public double ReplayGainAlbum => _taggedFile.Tag.ReplayGainAlbumGain;

    /// <summary>
    /// Gets text summary of track and album ReplayGain data.
    /// </summary>
    public string ReplayGainSummary()
    {
        const string noData = "———";
        var trackGain = double.IsNaN(ReplayGainTrack) ? noData : ReplayGainTrack.ToString();
        var albumGain = double.IsNaN(ReplayGainAlbum) ? noData : ReplayGainAlbum.ToString();
        return $"Track: {trackGain}  |  Album: {albumGain}";
    }

    /// <summary>
    /// The embedded image for the album, represented as an array of bytes or,
    /// if none, an empty array.
    /// </summary>
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
    /// Save pending tag data updates to the file.
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
    /// <returns>A collection of MediaFile.</returns>
    public static ImmutableList<MediaFile> PopulateFileData(string path, bool searchSubDirectories = false)
    {
        // If the path is a directory
        if (System.IO.Directory.Exists(path))
        {
            var mediaFiles = new List<MediaFile>();

            var fileNames = System.IO.Directory
                .EnumerateFiles(path,
                                "*.*",
                                searchSubDirectories
                                    ? System.IO.SearchOption.AllDirectories
                                    : System.IO.SearchOption.TopDirectoryOnly)
                .Where(IOUtilities.IsSupportedFileExtension)
                .ToArray();

            foreach (var fileName in fileNames)
            {
                try
                {
                    mediaFiles.Add(MediaFileFactory.CreateFileData(fileName));
                }
                catch (Exception) { }
            }

            return mediaFiles.OrderBy(f => f.Path)
                                .AsEnumerable()
                                .ToImmutableList();
        }

        // Otherwise, if the path is a file
        if (System.IO.File.Exists(path))
        {
            return new MediaFile[] { MediaFileFactory.CreateFileData(path) }.ToImmutableList();
        }

        throw new InvalidOperationException($"The path \"{path}\" was invalid.");
    }

    /// <summary>
    /// Generates and returns a list of tags that are populated in the file.
    /// </summary>
    public ImmutableList<string> PopulatedTagNames()
    {
        List<string> tags = new();

        if (HasAnyValues(this.AlbumArtists))
            tags.Add("ALBUMARTISTS");
        if (HasAnyValues(this.Artists))
            tags.Add("ARTISTS");
        if (!string.IsNullOrWhiteSpace(this.Album))
            tags.Add("ALBUM");
        if (!string.IsNullOrWhiteSpace(this.Title))
            tags.Add("TITLE");
        if (this.Year != 0)
            tags.Add("YEAR");
        if (this.TrackNo != 0)
            tags.Add("TRACK");

        return tags.ToImmutableList();
    }

    /// <summary>
    /// Specifies whether the given collection contains any non-whitespace values.
    /// </summary>
    public static bool HasAnyValues(IEnumerable<string> tagValues)
    {
        if (tagValues?.Any() != true)
            return false;

        var asString = string.Concat(tagValues);

        if (string.IsNullOrWhiteSpace(asString))
            return false;

        if (asString.Contains("<unknown>"))
            return false;

        return true;
    }
}
