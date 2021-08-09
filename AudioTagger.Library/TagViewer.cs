using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public partial class TagViewer : IPathProcessor
    {
        public virtual void Start(IReadOnlyCollection<MediaFile> mediaFiles, IPrinter printer)
        {
            foreach (var mediaFile in mediaFiles)
            {
                try
                {
                    printer.Print(OutputLine.GetTagPrintedLines(mediaFile));
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
