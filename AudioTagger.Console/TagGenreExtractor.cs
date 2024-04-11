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
        if (settings.ResetSavedArtistGenres)
        {
            printer.Print("Will reset any existing genre data.");
            existingGenres = [];
        }
        else
        {
            var readResult = GenreService.Read(settings.ArtistGenreCsvFilePath);
            if (readResult.IsSuccess)
            {
                printer.Print($"Will update \"{settings.ArtistGenreCsvFilePath}\".");
                existingGenres = readResult.Value;
                printer.Print($"Found {existingGenres.Count} genres.");
            }
            else
            {
                printer.Print($"Will create \"{settings.ArtistGenreCsvFilePath}\".");
                existingGenres = [];
            }
        }

        var latestGenres =
            mediaFiles
                .Where(f => f.Genres.Any() && f.Artists.Any())
                .GroupBy(f =>
                    f.Artists[0].Trim(), // Only the first artist.
                    f => f.Genres.GroupBy(g => g)
                                 .OrderByDescending(grp => grp.Count())
                                 .Select(grp => grp.Key)
                                 .First() // Keep only the most populous genre.
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

        WriteSummary(existingGenres.Count, mergedGenres.Count, printer);

        Result writeResult = GenreService.Write(settings.ArtistGenreCsvFilePath, mergedGenres);

        if (writeResult.IsSuccess)
            printer.Success($"Genres written to \"{settings.ArtistGenreCsvFilePath}\" in {watch.ElapsedFriendly}.");
        else
            printer.Error(writeResult.Errors.First().Message);

        static void WriteSummary(int beforeCount, int afterCount, IPrinter printer)
        {
            int countDiff = afterCount - beforeCount;
            var actionTaken = countDiff < 0 ? "removed" : "added";
            var beforeVsAfter = $"{beforeCount:#,##0} → {afterCount:#,##0}";
            var diffSummary = $"In total, {Math.Abs(countDiff)} genres to be {actionTaken} ({beforeVsAfter}).";
            printer.Print(diffSummary);
        }
    }
}
