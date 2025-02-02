namespace AudioTagger.Library.MediaFiles;

public interface IMediaData
{
    public string Title { get; }
    public string[] AlbumArtists { get; }
    public string[] Artists { get; }
    public string Album { get; }
    public uint Year { get; }
    public uint TrackNo { get; }
    public TimeSpan Duration { get; }
    public string[] Genres { get; }
    public string[] Composers { get; }
    public string Lyrics { get; }
    public string Comments { get; }
    public string Description { get; }
    public int BitRate { get; }
    public int SampleRate { get; }
    public double ReplayGainTrack { get; }
    public double ReplayGainAlbum { get; }
    public byte[] AlbumArt { get; }
}
