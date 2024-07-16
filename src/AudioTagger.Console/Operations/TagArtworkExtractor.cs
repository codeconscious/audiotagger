using AudioTagger.Library;
using FilesGroupedByDir = System.Linq.IGrouping<string?, AudioTagger.Library.MediaFiles.MediaFile>;
using FilesGroupedByCount = System.Linq.IGrouping<int, AudioTagger.Library.MediaFiles.MediaFile>;

namespace AudioTagger.Console.Operations;

public sealed class TagArtworkExtractor : IPathOperation
{
    private static readonly string _artworkFileName = "cover.jpg";

    private static bool AreAllSame(IEnumerable<string> items) =>
        items.Distinct().Count() == 1;

    private static bool IsAllArtUnique(
        FilesGroupedByDir fileGroup,
        IEnumerable<FilesGroupedByCount> filesGroupedByCount)
    {
        return
            fileGroup.Count() > 1 &&
            filesGroupedByCount.All(g => g.Count() == 1);
    }

    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        var watch = new Watch();

        var filesWithArt = mediaFiles.Where(f => f.HasAlbumArt());
        if (filesWithArt.None())
        {
            printer.Warning($"No artwork found in any files in \"{workingDirectory.FullName}\".");
            return;
        }

        var filesGroupedByDir = filesWithArt.GroupBy(m => m.FileInfo.DirectoryName);
        foreach (FilesGroupedByDir fileGroup in filesGroupedByDir)
        {
            ProcessDirectory(fileGroup, printer);
        }

        printer.Print($"Done in {watch.ElapsedFriendly}.");
    }

    private static void ProcessDirectory(FilesGroupedByDir fileGroup, IPrinter printer)
    {
        printer.Print($"Processing \"{fileGroup.Key}\"...");

        if (fileGroup.None(f => f.HasAlbumArt()))
        {
            printer.Warning($"No artwork found in this directory.");
            return;
        }

        var albumNames = fileGroup.Select(file => file.Album);
        var artistNames = fileGroup
            .Select(file => {
                return file.AlbumArtists.FirstOrDefault()
                    ?? file.Artists.FirstOrDefault()
                    ?? "None";
            });

        if (!AreAllSame(artistNames) || !AreAllSame(albumNames))
        {
            printer.Warning($"Skipping directory \"{fileGroup.Key}\" because either the artists or albums are not unique.");
            return;
        }

        var filesGroupedByArtSize = fileGroup.GroupBy(f => f.AlbumArt.Length, f => f);
        int mostCommonArtCount = filesGroupedByArtSize.Max(a => a.Count());
        var filesWithMostCommonArt = filesGroupedByArtSize.Where(a => a.Count() == mostCommonArtCount);

        if (IsAllArtUnique(fileGroup, filesWithMostCommonArt))
        {
            printer.Warning("All of the artwork is unique, so will not extract any artwork.");
            return;
        }

        if (filesWithMostCommonArt.Count() != 1)
        {
            printer.Warning("More than one image is most populous, but only one will be extracted.");
        }

        var filesWithChosenMostCommonArt = filesWithMostCommonArt.First();

        ExtractArtwork(filesWithChosenMostCommonArt, printer);

        foreach (MediaFile file in filesWithChosenMostCommonArt)
        {
            RemoveArtworkAndRewriteTags(file, printer);
        }
    }

    private static void ExtractArtwork(FilesGroupedByCount fileGroup, IPrinter printer)
    {
        MediaFile artSourceFile = fileGroup.First();
        string directoryName = artSourceFile.FileInfo.DirectoryName!;

        var extractResult = artSourceFile.ExtractArtworkToFile(directoryName, _artworkFileName);
        if (extractResult.IsSuccess)
        {
            printer.Print($"Saved most common artwork to \"{_artworkFileName}\" in the same directory.");
        }
        else
        {
            printer.FirstError(extractResult, "Artwork extraction error:");
        }
    }

    private static void RemoveArtworkAndRewriteTags(MediaFile file, IPrinter printer)
    {
        file.RemoveAlbumArt();
        var saveResult = file.SaveUpdates();
        if (saveResult.IsFailed)
        {
            printer.FirstError(saveResult);
            return;
        }

        var rewriteResult = file.RewriteFileTags();
        if (rewriteResult.IsFailed)
        {
            printer.FirstError(rewriteResult);
            return;
        }

        printer.Print($"Removed artwork from \"{file.FileNameOnly}\"");
    }
}
