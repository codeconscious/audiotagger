namespace AudioTagger.Console;

public class TagViewer : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IRegexCollection regexCollection,
                      IPrinter printer,
                      Settings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(mediaFiles);

        foreach (var mediaFile in mediaFiles)
        {
            try
            {
                //printer.Print(OutputLine.GetTagPrintedLines(mediaFile));
                var viewer = new MediaFileViewer();
                viewer.PrintFileDetails(mediaFile);

#if _WINDOWS
                if (mediaFile.AlbumArt.Length > 0)
                   Graphics.ConsoleWriteImage(mediaFile.AlbumArt);
#endif
            }
            catch (TagLib.CorruptFileException e)
            {
                printer.Error("The file's tag metadata was corrupt: " + e.Message);
                continue;
            }
            catch (Exception e)
            {
                printer.Error($"An unexpected error occurred with file {mediaFile.FileNameOnly}: " + e.Message);
                continue;
            }
        }
    }
}
