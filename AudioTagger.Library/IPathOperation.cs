using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public interface IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> filesData, IPrinter printer);
    }
}
