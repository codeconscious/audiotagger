using System.IO;

namespace AudioTagger.Library.MediaFiles;

public static class MediaFileFactory
{
    public static MediaFile CreateFileData(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(nameof(filePath));
        }

        var taggedFile = TagLib.File.Create(filePath);

        return new MediaFile(filePath, taggedFile);
    }
}
