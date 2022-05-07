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
            /// The file name from which the other expected values while be parsed, as possible.
            /// </summary>
            public string FileName { get; set; }

            // Expected values:
            public string Title { get; set; }
            public List<string> Artists { get; set; }
            public string Album { get; set; }
            public string Disc { get; set; }
            public string Track { get; set; }
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
                yield return new object[]
                {
                    new ParsedItem("吉田拓郎 - 夏休み [1971] {歌謡曲}.mp3")
                    {
                        Artists = new List<string> { "吉田拓郎" },
                        Title = "夏休み",
                        Year = "1971",
                        Genres = new List<string> { "歌謡曲" }
                    }
                };

                // yield return new object[]
                // {
                //     new ParsedItem("Michael Jackson - Thriller - 1.2 - Beat It [2005] {Pop}.mp4")
                //     {
                //         Title = "Beat It",
                //         Artists = new List<string> { "Michael Jackson" },
                //         Album = "Thriller",
                //         Disc = "1",
                //         Track = "2",
                //         Year = "2005",
                //         Genres = new List<string> { "Pop"}
                //     }
                // };

                // yield return new object[]
                // {
                //     new ParsedItem("Michael Jackson - Thriller - 2 - Beat It [2005] {Pop}.mp4")
                //     {
                //         Title = "Beat It",
                //         Artists = new List<string> { "Michael Jackson" },
                //         Album = "Thriller",
                //         Track = "2",
                //         Year = "2005",
                //         Genres = new List<string> { "Pop"}
                //     }
                // };

                // yield return new object[]
                // {
                //     new ParsedItem("Michael Jackson - Thriller - 2 - Beat It [2005].mp4")
                //     {
                //         Title = "Beat It",
                //         Artists = new List<string> { "Michael Jackson" },
                //         Album = "Thriller",
                //         Track = "2",
                //         Year = "2005",
                //     }
                // };

                yield return new object[]
                {
                    new ParsedItem("Michael Jackson - Thriller - 2 - Beat It {Pop}.mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" },
                        Album = "Thriller",
                        Track = "2",
                        Genres = new List<string> { "Pop"}
                    }
                };

                yield return new object[]
                {
                    new ParsedItem("Michael Jackson - Thriller - 2 - Beat It.mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" },
                        Album = "Thriller",
                        Track = "2",
                    }
                };

                // yield return new object[]
                // {
                //     new ParsedItem("Michael Jackson - Thriller - Beat It [2005] {Pop}.mp4")
                //     {
                //         Title = "Beat It",
                //         Artists = new List<string> { "Michael Jackson" },
                //         Album = "Thriller",
                //         Year = "2005",
                //         Genres = new List<string> { "Pop"}
                //     }
                // };

                // yield return new object[]
                // {
                //     new ParsedItem("Michael Jackson - Thriller - Beat It [2005].mp4")
                //     {
                //         Title = "Beat It",
                //         Artists = new List<string> { "Michael Jackson" },
                //         Album = "Thriller",
                //         Year = "2005",
                //     }
                // };

                yield return new object[]
                {
                    new ParsedItem("Michael Jackson - Thriller - Beat It {Pop}.mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" },
                        Album = "Thriller",
                        Genres = new List<string> { "Pop"}
                    }
                };

                // yield return new object[]
                // {
                //     new ParsedItem("Michael Jackson - Thriller - Beat It [2005] {Pop; Rock}.mp4")
                //     {
                //         Title = "Beat It",
                //         Artists = new List<string> { "Michael Jackson" },
                //         Album = "Thriller",
                //         Year = "2005",
                //         Genres = new List<string> { "Pop", "Rock" }
                //     }
                // };

                yield return new object[]
                {
                    new ParsedItem("Michael Jackson - Beat It [2005] {Pop}.mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" },
                        Year = "2005",
                        Genres = new List<string> { "Pop"}
                    }
                };

                yield return new object[]
                {
                    new ParsedItem("Michael Jackson - Beat It {Pop}.mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" },
                        Genres = new List<string> { "Pop"}
                    }
                };

                yield return new object[]
                {
                    new ParsedItem("Michael Jackson - Beat It [2005].mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" },
                        Year = "2005"
                    }
                };

                yield return new object[]
                {
                    new ParsedItem("Michael Jackson - Beat It.mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" }
                    }
                };

                // Extra spaces
                yield return new object[]
                {
                    new ParsedItem(" Michael Jackson  -  Beat It .mp4")
                    {
                        Title = "Beat It",
                        Artists = new List<string> { "Michael Jackson" }
                    }
                };

                yield return new object[]
                {
                    new ParsedItem("Beat It [2005] {Pop}.mp4")
                    {
                        Title = "Beat It",
                        Year = "2005",
                        Genres = new List<string> { "Pop"}
                    }
                };

                yield return new object[]
                {
                    new ParsedItem("Beat It [2005].mp4")
                    {
                        Title = "Beat It",
                        Year = "2005",
                    }
                };

                yield return new object[]
                {
                    new ParsedItem("Beat It {Pop}.mp4")
                    {
                        Title = "Beat It",
                        Genres = new List<string> { "Pop"}
                    }
                };

                yield return new object[]
                {
                     new ParsedItem("Beat It.mp4")
                     {
                         Title = "Beat It",
                     }
                };

                yield return new object[]
                {
                     new ParsedItem("82.99 F.M.mp3")
                     {
                         Title = "82.99 F.M",
                     }
                };

                yield return new object[]
                {
                     new ParsedItem("吉田拓郎 - 夏休み [1971] {歌謡曲}.mp3")
                     {
                         Artists = new List<string> { "吉田拓郎" },
                         Title = "夏休み",
                         Year = "1971",
                         Genres = new List<string> { "歌謡曲" }
                     }
                };

                yield return new object[]
                {
                     new ParsedItem("五木ひろし with 木の実ナナ - 居酒屋.ogg")
                     {
                         Artists = new List<string> { "五木ひろし with 木の実ナナ" },
                         Title = "居酒屋"
                     }
                };

                yield return new object[]
                {
                     new ParsedItem("Official髭男dism - I LOVE... [2020] {J-Pop}.mp3")
                     {
                         Artists = new List<string> { "Official髭男dism" },
                         Title = "I LOVE...",
                         Year = "2020",
                         Genres = new List<string> { "J-Pop" }
                     }
                };

                // Album first, various artists
                yield return new object[]
                {
                    new ParsedItem("Compilation of Jamz [2022] = Ororo Munroe - Storms of My Heart.mp3")
                    {
                        Album = "Compilation of Jamz",
                        Artists = new List<string> { "Ororo Munroe" },
                        Title = "Storms of My Heart",
                        Year = "2022"
                    }
                };

                // Album first, various artists
                yield return new object[]
                {
                    new ParsedItem("Compilation of Jamz [2022] = Ororo Munroe - Storms of My Heart {World}.mp3")
                    {
                        Album = "Compilation of Jamz",
                        Artists = new List<string> { "Ororo Munroe" },
                        Title = "Storms of My Heart",
                        Year = "2022",
                        Genres = new List<string> { "World" }
                    }
                };

                // Album first, various artists, no year, superfluous track number
                yield return new object[]
                {
                    new ParsedItem("最強の名曲集 = 020 20. 坂本龍馬 - 南方仁のとの会話.mp3")
                    {
                        Album = "最強の名曲集",
                        Artists = new List<string> { "坂本龍馬" },
                        Title = "南方仁のとの会話",
                        Track = "20"
                    }
                };

                // Album first, various artists, superfluous track number
                yield return new object[]
                {
                    new ParsedItem("最強の名曲集 [2022] = 020 20. 坂本龍馬 - 南方仁のとの会話.mp3")
                    {
                        Album = "最強の名曲集",
                        Artists = new List<string> { "坂本龍馬" },
                        Title = "南方仁のとの会話",
                        Year = "2022",
                        Track = "20"
                    }
                };

                // Test accented characters
                yield return new object[]
                {
                     new ParsedItem("Scott Summers - Size X Jeans - Eyé On You.mp3")
                     {
                         Artists = new List<string> { "Scott Summers" },
                         Album = "Size X Jeans",
                         Title = "Eyé On You"
                     }
                };

                // Contains "ft."
                yield return new object[]
                {
                     new ParsedItem("Natalia Lafourcade - Tú Me Acostumbraste ft. Omara Portuondo (En Manos de Los Macorinos) (Cover).mp3")
                     {
                         Artists = new List<string> { "Natalia Lafourcade" },
                         Title = "Tú Me Acostumbraste ft. Omara Portuondo (En Manos de Los Macorinos) (Cover)",
                     }
                };

                // Album-based
                yield return new object[]
                {
                     new ParsedItem("Rinsyoe Kida, Akira Ishikawa - Tsugaru Jongara Bushi: Drum & Tsugaru Jamisen [1973] - 01 - 津軽じょんがら節.mp3")
                     {
                         Artists = new List<string> { "Rinsyoe Kida, Akira Ishikawa" },
                         Album = "Tsugaru Jongara Bushi: Drum & Tsugaru Jamisen",
                         Title = "津軽じょんがら節",
                         Track = "1",
                         Year = "1973",
                     }
                };

                // "S.O.S." parsing issues
                yield return new object[]
                {
                     new ParsedItem("Arbitrarious - S.O.S. (@地球) [1865] {International}.mp3")
                     {
                         Artists = new List<string> { "Arbitrarious" },
                         Title = "S.O.S. (@地球)",
                         Year = "1865",
                         Genres = new List<string> { "International" }
                     }
                };

                // "...WHY" parsing issues
                yield return new object[]
                {
                     new ParsedItem("Paft Dunk - Ongakooz [1756] - 02 - ...WHY.mp3")
                     {
                         Artists = new List<string> { "Paft Dunk" },
                         Album = "Ongakooz",
                         Title = "...WHY",
                         Year = "1756",
                         Track = "2"                     }
                };

                // "WHY..." potential parsing issues
                yield return new object[]
                {
                     new ParsedItem("Paft Dunk - Ongakooz [1756] - 02 - WHY....mp3")
                     {
                         Artists = new List<string> { "Paft Dunk" },
                         Album = "Ongakooz",
                         Title = "WHY...",
                         Year = "1756",
                         Track = "2"                     }
                };

                // "...WHY..." potential parsing issues
                yield return new object[]
                {
                     new ParsedItem("Paft Dunk - Ongakooz [1756] - 02 - ...WHY....mp3")
                     {
                         Artists = new List<string> { "Paft Dunk" },
                         Album = "Ongakooz",
                         Title = "...WHY...",
                         Year = "1756",
                         Track = "2"                     }
                };

                yield return new object[]
                {
                     new ParsedItem("真剣赫怒 - What's This, Mr.Random [2030].mp3")
                     {
                         Artists = new List<string> { "真剣赫怒" },
                         Title = "What's This, Mr.Random",
                         Year = "2030"
                    }
                };

                yield return new object[]
                {
                     new ParsedItem("全世界の生物の魂 - If You... (Remix).mp3")
                     {
                         Artists = new List<string> { "全世界の生物の魂" },
                         Title = "If You... (Remix)",
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestDataSet))]
        public void CanParseValidFileNames(ParsedItem expected)
        {
            var match = RegexCollection.GetFirstMatch(expected.FileName);

            var matchedTags = match.Groups
                                   .OfType<Group>()
                                   .Where(g => g.Success);

            var matchedData = new UpdatableFields(matchedTags);

            Assert.NotNull(matchedData);

            if (expected.Artists?.Any() == true)
            {
                Assert.Equal(expected.Artists.Count, matchedData.Artists.Length);
                for (var i = 0; i < expected.Artists.Count; i++)
                {
                    Assert.Equal(expected.Artists[i], matchedData.Artists[i].Trim());
                }
            }
            else
            {
                Assert.Null(matchedData.Artists);
            }

            // There should always be a title.
            Assert.Equal(expected.Title, matchedData.Title.Trim());

            // TODO: Add a Disc and Track property to matched data.
            //Assert.Equal(data.Disc, matchedData.Disc);
            //Assert.Equal(data.Track, matchedData.Track);

            if (expected.Year != null)
                Assert.Equal(expected.Year ?? null, matchedData.Year.ToString().Trim());
            else
                Assert.Null(matchedData.Year);

            if (expected.Genres?.Any() == true)
            {
                Assert.Equal(expected.Genres.Count, matchedData.Genres.Length);
                for (var i = 0; i < expected.Genres.Count; i++)
                {
                    Assert.Equal(expected.Genres[i], matchedData.Genres[i].Trim());
                }
            }
            else
            {
                Assert.Null(matchedData.Genres);
            }
        }
    }
}
