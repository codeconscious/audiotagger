namespace AudioTagger.Library.MediaFiles;

public interface IMediaData
{
    public string Title { get; set; }
    public string[] AlbumArtists { get; set; }
    public string[] Artists { get; set; }
    public string Album { get; set; }
    public uint Year { get; set; }
    public uint TrackNo { get; set; }
    public TimeSpan Duration { get; }
    public string[] Genres { get; set; }
    public string[] Composers { get; set; }
    public string Lyrics { get; set; }
    public string Comments { get; set; }
    public string Description { get; set; }
    public int BitRate { get; }
    public int SampleRate { get; }
    public double ReplayGainTrack { get; }
    public double ReplayGainAlbum { get; }
    public byte[] AlbumArt { get; }
}
