using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public interface IPathProcessor
    {
        public void Start(IReadOnlyCollection<MediaFile> filesData, IPrinter printer);
    }
}
