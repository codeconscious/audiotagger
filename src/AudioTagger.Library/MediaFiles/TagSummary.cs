namespace AudioTagger.Library.MediaFiles;

public class TagSummary : IMediaData
{
    public string Title { get; } = string.Empty;
    public string[] AlbumArtists { get; } = [];
    public string[] Artists { get; } = [];
    public string Album { get; } = string.Empty;
    public uint Year { get; }
    public uint TrackNo { get; }
    public TimeSpan Duration { get; }
    public string[] Genres { get; } = [];
    public string[] Composers { get; } = [];
    public string Lyrics { get; } = string.Empty;
    public string Comments { get; } = string.Empty;
    public string Description { get; } = string.Empty;
    public int BitRate { get; }
    public int SampleRate { get; }
    public double ReplayGainTrack { get; }
    public double ReplayGainAlbum { get; }
    public byte[] AlbumArt { get; } = [];
}
