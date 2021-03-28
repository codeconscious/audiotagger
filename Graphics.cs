using System;
using System.Linq;
using System.IO;
using System.Drawing;

namespace AudioTagger
{
    public static class Graphics
    {
        /// <summary>
        /// Print an approximation of an image to the console.
        /// Prints nothing on unsupported platforms.
        /// </summary>
        public static void ConsoleWriteImage(byte[] bytes, byte? desiredMaxWidth = null)
        {
            var maxWidth = GetValidImageWidth(desiredMaxWidth);

            ConsoleWriteImage(bytes, maxWidth);
        }

        /// <summary>
        /// Print an approximation of an image to the console.
        /// Prints nothing on unsupported platforms.
        /// </summary>
        /// <see cref="https://www.hanselman.com/blog/how-do-you-use-systemdrawing-in-net-core"/>
        /// <seealso cref="https://stackoverflow.com/a/33689107/11767771"/>
        /// <param name="bytes">An array of bytes corrresponding to an image.</param>
        private static void ConsoleWriteImage(byte[] bytes, int maxWidth)
        {
            Bitmap bitmap;

            using (var ms = new MemoryStream(bytes))
            {
                bitmap = new Bitmap(ms);
            }

            ConsoleWriteImage(bitmap, maxWidth);
        }

        /// <summary>
        /// Print an approximation of an image to the console.
        /// Does nothing on unsupported platforms.
        /// </summary>
        /// <see cref="https://www.hanselman.com/blog/how-do-you-use-systemdrawing-in-net-core"/>
        /// <seealso cref="https://stackoverflow.com/a/33689107/11767771"/>
        /// <param name="bitmap"></param>
        private static void ConsoleWriteImage(Bitmap bitmap, int maxWidth)
        {
            try
            {
                var percent = Math.Min(decimal.Divide(maxWidth, bitmap.Width),
                                       decimal.Divide(maxWidth, bitmap.Height));

                var resSize = new Size((int)(bitmap.Width * percent),
                                       (int)(bitmap.Height * percent));

                static int ToConsoleColor(Color c)
                {
                    int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
                    index |= (c.R > 64) ? 4 : 0;
                    index |= (c.G > 64) ? 2 : 0;
                    index |= (c.B > 64) ? 1 : 0;
                    return index;
                }

                var bmp = new Bitmap(bitmap, resSize.Width * 2, resSize.Height * 2);

                for (int i = 0; i < resSize.Height; i++)
                {
                    for (int j = 0; j < resSize.Width; j++)
                    {
                        Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmp.GetPixel(j * 2, i * 2));
                        Console.BackgroundColor = (ConsoleColor)ToConsoleColor(bmp.GetPixel(j * 2, i * 2 + 1));
                        Console.Write("▀");

                        Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmp.GetPixel(j * 2 + 1, i * 2));
                        Console.BackgroundColor = (ConsoleColor)ToConsoleColor(bmp.GetPixel(j * 2 + 1, i * 2 + 1));
                        Console.Write("▀");

                        
                    }

                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
            catch
            {
                // The platform is unsupported, so do nothing. (Look into libgdiplus or other options.)
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Gets a valid maximum size for the width of the image.
        /// Intended to be used for printing images to the console.
        /// </summary>
        /// <param name="desiredMaxSize"></param>
        /// <returns>A valid image width.</returns>
        private static int GetValidImageWidth(byte? desiredMaxSize)
        {
            // Division by 2 is needed because images with a width of 60 fits on a console of width 120.
            var maxPossibleSize = (int)Math.Floor(Console.WindowWidth / 2f);

            return desiredMaxSize == null || desiredMaxSize.Value > maxPossibleSize
                ? maxPossibleSize
                : desiredMaxSize.Value;
        }
    }
}