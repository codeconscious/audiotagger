using AudioTagger.Library.Genres;

namespace AudioTagger.Console;

public sealed class TagGenreExtractor : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        if (string.IsNullOrWhiteSpace(settings.ArtistGenreCsvFilePath))
        {
            printer.Error("You must specify a .csv containing artist and genre data in your settings file under the 'artistGenreCsvFilePath' key.");
            return;
        }

        Watch watch = new();

        Dictionary<string, string> existingGenres;
        var readResult = GenreService.Read(settings.ArtistGenreCsvFilePath);
        if (readResult.IsSuccess)
        {
            existingGenres = readResult.Value;
            printer.Print($"Found {existingGenres.Count} genres.");
            printer.Print($"Will update \"{settings.ArtistGenreCsvFilePath}\".");
        }
        else
        {
            existingGenres = [];
            printer.Print($"Will create \"{settings.ArtistGenreCsvFilePath}\".");
        }

        var latestGenres =
            mediaFiles
                .Where(f => f.Genres.Any() && f.Artists.Any())
                .GroupBy(f =>
                    f.Artists[0].Trim(), // Only the first artist (though it might be nice to process all someday)
                    f => f.Genres.GroupBy(g => g) // Get most populous...
                                 .OrderByDescending(grp => grp.Count()) // ...and keep them at the top.
                                 .Select(grp => grp.Key)
                                 .First() // Keep only the single most populous genre.
                                 .Trim())
                .ToImmutableSortedDictionary(
                    f => f.Key,
                    f => f.First()
                );

        printer.Print($"Found {latestGenres.Count:#,##0} unique artists with genres.");

        var mergedGenres = latestGenres
            .Concat(existingGenres)
            .GroupBy(e => e.Key)
            .ToDictionary(g => g.Key, g => g.First().Value); // Prioritize the first dictionary's values.

        int countDiff = mergedGenres.Count - existingGenres.Count;
        var diffSummary = $"In total, {Math.Abs(countDiff)} genres to be {(countDiff < 0 ? "removed" : "added")}.";
        printer.Print(diffSummary);

        Result writeResult = GenreService.Write(settings.ArtistGenreCsvFilePath, mergedGenres);

        if (writeResult.IsSuccess)
            printer.Success($"Genres written to \"{settings.ArtistGenreCsvFilePath}\" in {watch.ElapsedFriendly}.");
        else
            printer.Error(writeResult.Errors.First().Message);
    }
}
