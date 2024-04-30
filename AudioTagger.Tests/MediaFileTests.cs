using AudioTagger.Library.MediaFiles;
using Xunit;

namespace AudioTagger.Tests;

public sealed class MediaFileTests
{
    public sealed class ArtistSummary
    {
        private static readonly string Separator = "; ";

        [Fact]
        public void ContainsAllArtists_ReturnsExpectedString()
        {
            string[] albumArtists = ["Album Artist A"];
            string[] artists = ["Artist 1", "Artist 2"];
            string expected = "Album Artist A (Artist 1; Artist 2)";
            string actual = albumArtists.JoinWith(artists, Separator);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ContainsOnlyAlbumArtists_ReturnsExpectedString()
        {
            string[] albumArtists = ["Album Artist"];
            string[] artists = [];
            string expected = "Album Artist";
            string actual = albumArtists.JoinWith(artists, Separator);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ContainsOnlyTrackArtists_ReturnsExpectedString()
        {
            string[] albumArtists = [];
            string[] artists = ["Artist 1", "Artist 2"];
            string expected = "Artist 1; Artist 2";
            string actual = albumArtists.JoinWith(artists, Separator);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ContainsNoArtists_ReturnsEmptyString()
        {
            string[] albumArtists = [];
            string[] artists = [];
            string expected = string.Empty;
            string actual = albumArtists.JoinWith(artists, Separator);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ContainsNullCollections_ReturnsEmptyString()
        {
            string[]? albumArtists = null;
            string[]? artists = null;
            string expected = string.Empty;
            string actual = albumArtists!.JoinWith(artists!, Separator);
            Assert.Equal(expected, actual);
        }
    }
}
