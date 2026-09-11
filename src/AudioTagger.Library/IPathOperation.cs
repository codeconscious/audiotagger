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

    /// <summary>
    /// The name of the operation. It should be grammatically able to follow
    /// the word "starting" -- e.g., "starting tag updater".
    /// </summary>
    public abstract string Name();
}
