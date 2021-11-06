using System;
using Xunit;
using AudioTagger;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace AudioTagger.Tests
{
    public class RegexTests
    {
        public class ParsedItem
        {
            /// <summary>
            /// The file name from which the other values while be parsed, as possible.
            /// </summary>
            public string FileName { get; set; }

            // Expected values:
            public List<string> Artists { get; set; }
            public string Album { get; set; }
            public string Disc { get; set; }
            public string Track { get; set; }
            public string Title { get; set; }
            public string Year { get; set; }
            public List<string> Genres { get; set; }

            public ParsedItem(string fileName)
            {
                FileName = fileName;
            }
        }

        private class TestDataSet : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new ParsedItem("吉田拓郎 - 夏休み [1971] {歌謡曲}.mp3")
                    {
                        Artists = new List<string> { "吉田拓郎" },
                        Title = "夏休み",
                        Year = "1971",
                        Genres = new List<string> { "歌謡曲" }
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestDataSet))]
        public void CanParseFiles_ArtistsTitleYearGenres(ParsedItem data)
        {
            var match = RegexCollection.GetFirstMatch(data.FileName);

            var matchedTags = match.Groups
                                   .OfType<Group>()
                                   .Where(g => g.Success);

            var matchedData = new UpdatableFields(matchedTags);

            Assert.NotNull(matchedData);

            Assert.Equal(data.Artists.Count, matchedData.Artists.Length);
            for (var i = 0; i < data.Artists.Count; i++)
            {
                Assert.Equal(data.Artists[i], matchedData.Artists[i]);
            }

            Assert.Equal(data.Title, matchedData.Title);

            Assert.Equal(data.Year, matchedData.Year.ToString());

            Assert.Equal(data.Genres.Count, matchedData.Genres.Length);
            for (var i = 0; i < data.Genres.Count; i++)
            {
                Assert.Equal(data.Genres[i], matchedData.Genres[i]);
            }
        }
    }
}
