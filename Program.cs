using System;
using TagLib;

namespace Music
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var filename in args)
            {
                if (string.IsNullOrWhiteSpace(filename))
                {
                    Console.WriteLine("A filename was not entered...");
                    continue;
                }

                var taggedFile = TagLib.File.Create(filename);
                string title = taggedFile.Tag.Title;
                TimeSpan duration = taggedFile.Properties.Duration;
                Console.WriteLine();
                Console.WriteLine($"  Title: {title}");
                Console.WriteLine($"  Artist(s): {string.Join(", ", taggedFile.Tag.Performers)}");
                Console.WriteLine($"  Duration: {duration:c}");
                Console.WriteLine($"  Genre: {string.Join(", ", taggedFile.Tag.Genres)}");
                Console.WriteLine($"  Bitrate: {taggedFile.Properties.AudioBitrate}");
                Console.WriteLine($"  Sample Rate: {taggedFile.Properties.AudioSampleRate:#,##0}");
                //Console.WriteLine();
                Console.WriteLine($"  Composers: {taggedFile.Tag.JoinedComposers}");
                Console.WriteLine();

                // change title in the file
                //tfile.Tag.Title = "my new title";
                //tfile.Save();
            }
        }
    }
}
