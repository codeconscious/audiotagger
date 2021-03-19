using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

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
                }
                catch (TagLib.CorruptFileException e)
                {
                    Printer.Error("The file's tag metadata was corrupt or missing." + e.Message);
                    continue;
                }
                catch (Exception e)
                {
                    Printer.Error("An unknown error occurred." + e.Message);
                    continue;
                }
            }
        }        
    }
}
