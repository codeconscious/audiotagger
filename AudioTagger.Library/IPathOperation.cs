using System.IO;

namespace AudioTagger;

public interface IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> filesData,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings? settings = null);
}
