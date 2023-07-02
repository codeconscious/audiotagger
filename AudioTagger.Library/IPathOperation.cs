using System.IO;
using AudioTagger.Library.MediaFiles;

namespace AudioTagger;

public interface IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> filesData,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings);
}
