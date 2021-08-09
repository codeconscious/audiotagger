using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public class TagViewer : IPathProcessor
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles, IPrinter printer)
        {
            foreach (var mediaFile in mediaFiles)
            {
                try
                {
                    printer.Print(OutputLine.GetTagPrintedLines(mediaFile));
                    // foreach (var line in )

                    // TODO: Check ahead of time if images are supported.
                    if (mediaFile.AlbumArt.Length > 0)
                        Graphics.ConsoleWriteImage(mediaFile.AlbumArt);
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
