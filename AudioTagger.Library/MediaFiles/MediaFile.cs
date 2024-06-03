using System.IO;
using FluentResults;

namespace AudioTagger.Library.MediaFiles;

public sealed class MediaFile
{
    public FileInfo FileInfo { get; }
    private TagLib.File _taggedFile;

    public MediaFile(string filePath, TagLib.File tabLibFile)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(tabLibFile);

        FileInfo = new FileInfo(filePath);
        _taggedFile = tabLibFile;
    }

    public string FileNameOnly => FileInfo.Name;

    public long FileSizeInBytes => FileInfo.Length;

    public string ParentDirectoryName => FileInfo.Directory!.Name;


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
                    value.Where(a => a.HasText())
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
                    value.Where(a => a.HasText())
                         .Select(a => a.Trim().Normalize())
                         .Distinct()
                         .ToArray();
    }

    /// <summary>
    /// A summary of both album artists and/or track artists; otherwise, an empty string.
    /// </summary>
    public string ArtistSummary => AlbumArtists.JoinWith(Artists, "; ");

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
        string trackGain = double.IsNaN(ReplayGainTrack) ? noData : ReplayGainTrack.ToString();
        string albumGain = double.IsNaN(ReplayGainAlbum) ? noData : ReplayGainAlbum.ToString();
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
    /// Removes album art from the file's tag data.
    /// </summary>
    /// <remarks>Padding space remains, so the file size is not reduced.</remarks>
    public void RemoveAlbumArt()
    {
        if (_taggedFile.Tag?.Pictures.Any() != true)
            return;

        _taggedFile.Tag.Pictures = [];
    }

    /// <summary>
    /// Save pending tag data updates to the file.
    /// </summary>
    public void SaveUpdates()
    {
        _taggedFile.Save();
        _taggedFile.Dispose();
    }

    /// <summary>
    /// Given a file path, returns the tag information from that file.
    /// </summary>
    public static Result<MediaFile> ReadFileTags(string filePath)
    {
        try
        {
            MediaFile mediaFile = MediaFileFactory.CreateFileData(filePath);
            return Result.Ok(mediaFile);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Could not read tags of file \"{filePath}\": {ex.Message}");
        }
    }

    /// <summary>
    /// Delete and readd the same tag data to the file. Useful for removing remaining
    /// padding from deleted artwork, etc.
    /// </summary>
    public Result RewriteFileTags()
    {
        try
        {
            TagLib.Tag tempTags = new TagLib.Id3v2.Tag();
            _taggedFile.Tag.CopyTo(tempTags, true);
            _taggedFile.RemoveTags(TagLib.TagTypes.AllTags);
            SaveUpdates();

            _taggedFile = TagLib.File.Create(FileInfo.FullName);
            tempTags.CopyTo(_taggedFile.Tag, true);
            SaveUpdates();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Save error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates and returns a list of tag frames in the file that are populated.
    /// </summary>
    public ImmutableList<string> PopulatedTagNames()
    {
        List<string> tags = [];

        if (HasAnyValues(this.AlbumArtists))
            tags.Add("ALBUMARTISTS");
        if (HasAnyValues(this.Artists))
            tags.Add("ARTISTS");
        if (this.Album.HasText())
            tags.Add("ALBUM");
        if (this.Title.HasText())
            tags.Add("TITLE");
        if (this.Year != 0)
            tags.Add("YEAR");
        if (this.TrackNo != 0)
            tags.Add("TRACK");

        return [.. tags];
    }

    /// <summary>
    /// Specifies whether the given collection contains any non-whitespace values.
    /// </summary>
    public static bool HasAnyValues(IEnumerable<string> tagValues)
    {
        if (tagValues?.Any() != true)
            return false;

        string asString = string.Concat(tagValues);

        if (string.IsNullOrWhiteSpace(asString))
            return false;

        if (asString.Contains("<unknown>"))
            return false;

        return true;
    }
}
