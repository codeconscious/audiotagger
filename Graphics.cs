using System;
using System.Linq;
using System.IO;
using System.Drawing;

namespace AudioTagger
{
    public class Graphics
    {
        /// <summary>
        /// Print an approximation of an image to the console.
        /// Does nothing on unsupported platforms.
        /// </summary>
        /// <see cref="https://www.hanselman.com/blog/how-do-you-use-systemdrawing-in-net-core"/>
        /// <seealso cref="https://stackoverflow.com/a/33689107/11767771"/>
        /// <param name="bytes"></param>
        public static void ConsoleWriteImage(byte[] bytes)
        {
            try
            {
                Bitmap bitmap;

                using (var ms = new MemoryStream(bytes))
                {
                    bitmap = new Bitmap(ms);
                }

                ConsoleWriteImage(bitmap);
            }
            catch
            {
                // The platform is unsupported, so do nothing. (Look into libgdiplus or other options.)
            }
        }

        /// <summary>
        /// Print an approximation of an image to the console.
        /// Does nothing on unsupported platforms.
        /// </summary>
        /// <see cref="https://www.hanselman.com/blog/how-do-you-use-systemdrawing-in-net-core"/>
        /// <seealso cref="https://stackoverflow.com/a/33689107/11767771"/>
        /// <param name="bitmap"></param>
        public static void ConsoleWriteImage(Bitmap bitmap, byte maxSize = 25)
        {
            try
            {
                //int maxSize = 20; // 39;

                var percent = Math.Min(decimal.Divide(maxSize, bitmap.Width), decimal.Divide(maxSize, bitmap.Height));
            
                var resSize = new Size((int)(bitmap.Width * percent), (int)(bitmap.Height * percent));
            
                Func<System.Drawing.Color, int> ToConsoleColor = c =>
                {
                    int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
                    index |= (c.R > 64) ? 4 : 0;
                    index |= (c.G > 64) ? 2 : 0;
                    index |= (c.B > 64) ? 1 : 0;
                    return index;
                };

                var bmpMin = new Bitmap(bitmap, resSize.Width, resSize.Height);
                var bmpMax = new Bitmap(bitmap, resSize.Width * 2, resSize.Height * 2);
            
                for (int i = 0; i < resSize.Height; i++)
                {
                    for (int j = 0; j < resSize.Width; j++)
                    {
                        Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmpMin.GetPixel(j, i));
                        Console.Write("██");
                    }

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("    ");

                    for (int j = 0; j < resSize.Width; j++)
                    {
                        Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2, i * 2));
                        Console.BackgroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2, i * 2 + 1));
                        Console.Write("▀");

                        Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2 + 1, i * 2));
                        Console.BackgroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2 + 1, i * 2 + 1));
                        Console.Write("▀");
                    }

                    Console.WriteLine();
                }
            }
            catch
            {
                // The platform is unsupported, so do nothing. (Look into libgdiplus or other options.)
            }
        }
    }
}