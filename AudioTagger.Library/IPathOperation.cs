using System.IO;
using AudioTagger.Library.MediaFiles;
using AudioTagger.Library.Settings;

namespace AudioTagger;

public interface IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> filesData,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer);
}
