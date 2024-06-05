namespace AudioTagger.Console;

public sealed class TagArtworkExtractor : IPathOperation
{
    private static readonly string _artworkFileNamePrefix = "cover";

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
            printer.Warning("No album art found in the files.");
            return;
        }

        var filesByDirectory = filesWithArtwork.GroupBy(m => m.FileInfo.DirectoryName);

        foreach (var dirGroup in filesByDirectory)
        {
            var dirName = dirGroup.Key;

            var artistNames = dirGroup
                .Select(file => {
                    return file.AlbumArtists.FirstOrDefault()
                        ?? file.Artists.FirstOrDefault()
                        ?? "None";
                });
            var albumNames = dirGroup.Select(file => file.Album);

            static bool AllUnique(IEnumerable<string> items) =>
                items.Distinct().Count() == 1;

            if (!AllUnique(artistNames) || !AllUnique(albumNames))
            {
                printer.Warning($"Skipping directory \"{dirName}\" because either the artists or albums are not unique.");
                continue;
            }

            var albumSizeAndArtGroup = dirGroup.GroupBy(g => g.AlbumArt.Length, g => g);
            var mostPopulousArtCount = albumSizeAndArtGroup.Max(a => a.Count());
            var mostPopulousArtGroups = albumSizeAndArtGroup.Where(a => a.Count() == mostPopulousArtCount);

            if (mostPopulousArtGroups.Count() == 0)
            {
                printer.Warning("No album art found in this directory.");
                return;
            }

            if (mostPopulousArtGroups.All(g => g.Count() == 1))
            {
                printer.Warning("All of the artwork is unique, so will not extract any artwork.");
                return;
            }

            if (mostPopulousArtGroups.Count() != 1)
            {
                printer.Warning("More than one artwork appears multiple times. Will only extract one.");
            }

            int failures = 0;
            var mostPopulousFirstGroup = mostPopulousArtGroups.First();
            foreach (var file in mostPopulousFirstGroup)
            {
                var directoryName = file.FileInfo.DirectoryName!;
                var result = file.ExtractArtworkToFile(directoryName, _artworkFileNamePrefix, ".jpg");
                if (result.IsSuccess)
                {
                    printer.Print($"Saved artwork to \"{_artworkFileNamePrefix}.jpg.");
                }
                else
                {
                    failures++;
                    printer.Error(result.Errors.First().Message);
                }
            }

            if (failures == 0)
            {
                foreach (var file in mostPopulousFirstGroup)
                {
                    file.RemoveAlbumArt();
                    var saveResult = file.SaveUpdates();
                    if (saveResult.IsFailed)
                    {
                        printer.Error(saveResult.Errors.First().Message);
                        continue;
                    }

                    var rewriteResult = file.RewriteFileTags();
                    if (rewriteResult.IsFailed)
                    {
                        printer.Error(rewriteResult.Errors.First().Message);
                        continue;
                    }

                    printer.Print($"Removed artwork from \"{file.FileNameOnly}\"");
                }
            }
            else
            {
                printer.Warning($"There were {failures} extraction error(s), so will not delete any artwork.");
            }

            printer.Print($"Done in {watch.ElapsedFriendly}.");
        }
    }
}
