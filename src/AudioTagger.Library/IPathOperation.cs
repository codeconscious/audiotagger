using System.IO;
using AudioTagger.Library.MediaFiles;
using AudioTagger.Library.UserSettings;

namespace AudioTagger.Library;

public interface IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer);
}
