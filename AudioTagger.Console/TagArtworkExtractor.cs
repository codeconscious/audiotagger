namespace AudioTagger.Console;

public sealed class TagArtworkExtractor : IPathOperation
{
    private static readonly string _artworkFileName = "cover.jpg";

    static bool AllUnique(IEnumerable<string> items) => items.Distinct().Count() == 1;

    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        var watch = new Watch();

        var filesWithArtwork = mediaFiles.Where(f => f.AlbumArt.Length != 0);
        if (!filesWithArtwork.Any())
        {
            printer.Warning("No artwork found in the files.");
            return;
        }

        var filesGroupedByDirectory = filesWithArtwork.GroupBy(m => m.FileInfo.DirectoryName);

        foreach (var fileGroup in filesGroupedByDirectory)
        {
            if (fileGroup.All(f => f.AlbumArt.Length == 0))
            {
                printer.Warning($"No artwork found in directory {fileGroup.Key}.");
                continue;
            }

            var albumNames = fileGroup.Select(file => file.Album);
            var artistNames = fileGroup
                .Select(file => {
                    return file.AlbumArtists.FirstOrDefault()
                        ?? file.Artists.FirstOrDefault()
                        ?? "None";
                });

            if (!AllUnique(artistNames) || !AllUnique(albumNames))
            {
                printer.Warning($"Skipping directory \"{fileGroup.Key}\" because either the artists or albums are not unique.");
                continue;
            }

            var filesGroupedByArtSize = fileGroup.GroupBy(f => f.AlbumArt.Length, f => f);
            var mostCommonArtCount = filesGroupedByArtSize.Max(a => a.Count());
            var filesWithMostCommonArt = filesGroupedByArtSize.Where(a => a.Count() == mostCommonArtCount);

            if (mediaFiles.Count > 1 && filesWithMostCommonArt.All(g => g.Count() == 1))
            {
                printer.Warning("All of the artwork is unique, so will not extract any artwork.");
                return;
            }

            if (filesWithMostCommonArt.Count() != 1)
            {
                printer.Warning("More than one artwork appears multiple times. Will only extract one.");
            }

            int failures = 0;
            var filesWithChosenMostCommonArt = filesWithMostCommonArt.First();

            foreach (var file in filesWithChosenMostCommonArt)
            {
                var directoryName = file.FileInfo.DirectoryName!;
                var extractResult = file.ExtractArtworkToFile(directoryName, _artworkFileName);

                if (extractResult.IsSuccess)
                {
                    printer.Print($"Saved artwork to \"{_artworkFileName}\".");
                }
                else
                {
                    failures++;
                    printer.FirstError(extractResult, "Artwork extraction error:");
                }
            }

            if (failures != 0)
            {
                printer.Warning($"There were {failures} extraction error(s), so will not delete any artwork.");
                printer.Print($"Done in {watch.ElapsedFriendly}.");
                return;
            }

            foreach (var file in filesWithChosenMostCommonArt)
            {
                file.RemoveAlbumArt();
                var saveResult = file.SaveUpdates();
                if (saveResult.IsFailed)
                {
                    printer.FirstError(saveResult);
                    continue;
                }

                var rewriteResult = file.RewriteFileTags();
                if (rewriteResult.IsFailed)
                {
                    printer.FirstError(rewriteResult);
                    continue;
                }

                printer.Print($"Removed artwork from \"{file.FileNameOnly}\"");
            }

            printer.Print($"Done in {watch.ElapsedFriendly}.");
        }
    }
}
