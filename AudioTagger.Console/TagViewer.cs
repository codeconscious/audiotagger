namespace AudioTagger.Console
{
    public class TagViewer : IPathOperation
    {
        public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                          DirectoryInfo workingDirectory,
                          IRegexCollection regexCollection,
                          IPrinter printer)
        {
            ArgumentNullException.ThrowIfNull(mediaFiles);

            foreach (var mediaFile in mediaFiles)
            {
                try
                {
                    //printer.Print(OutputLine.GetTagPrintedLines(mediaFile));
                    var viewer = new MediaFileViewer();
                    viewer.PrintFileDetails(mediaFile);

                    // TODO: Check ahead of time if images are supported.
                    //if (mediaFile.AlbumArt.Length > 0)
                    //    Graphics.ConsoleWriteImage(mediaFile.AlbumArt);
                }
                catch (TagLib.CorruptFileException e)
                {
                    printer.Error("The file's tag metadata was corrupt or missing: " + e.Message);
                    continue;
                }
                catch (Exception e)
                {
                    printer.Error($"An unknown error occurred with file {mediaFile.FileNameOnly}: " + e.Message);
                    continue;
                }
            }
        }
    }
}
