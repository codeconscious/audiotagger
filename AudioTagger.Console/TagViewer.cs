using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public class TagViewer : IPathProcessor
    {
        public void Start(IReadOnlyCollection<MediaFile> filesData, IPrinter printer)
        {
            foreach (var file in filesData)
            {
                try
                {
                    printer.GetTagPrintedLines(file);
                    // foreach (var line in )

                    // TODO: Check ahead of time if images are supported.
                    if (file.AlbumArt.Length > 0)
                        Graphics.ConsoleWriteImage(file.AlbumArt);
                }
                catch (TagLib.CorruptFileException e)
                {
                    printer.Error("The file's tag metadata was corrupt or missing: " + e.Message);
                    continue;
                }
                catch (Exception e)
                {
                    printer.Error("An unknown error occurred: " + e.Message);
                    continue;
                }
            }
        }
    }
}
