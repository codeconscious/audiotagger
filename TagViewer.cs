using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public class TagViewer : IPathProcessor
    {
        public void Start(IReadOnlyCollection<FileData> filesData)
        {
            foreach (var file in filesData)
            {
                try
                {
                    Printer.Print(file.GetTagPrintedLines());

                    if (file.AlbumArt.Length > 0)
                        Graphics.ConsoleWriteImage(file.AlbumArt);
                }
                catch (TagLib.CorruptFileException e)
                {
                    Printer.Error("The file's tag metadata was corrupt or missing: " + e.Message);
                    continue;
                }
                catch (Exception e)
                {
                    Printer.Error("An unknown error occurred: " + e.Message);
                    continue;
                }
            }
        }        
    }
}
