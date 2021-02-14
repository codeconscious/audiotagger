using System;
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

        public void SaveUpdates()
        {
            _tabLibFile.Save();
        }
    }
}
