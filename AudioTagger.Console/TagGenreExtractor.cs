using AudioTagger.Library.Genres;

namespace AudioTagger.Console;

public sealed class TagGenreExtractor : IPathOperation
{
    private static bool HasGenresAndArtists(MediaFile file) =>
        file.Genres.Length != 0 && file.Artists.Length != 0;

    private static string FirstArtistTrimmed(MediaFile file) =>
        file.Artists[0].Trim();

    private static string ArtistName(IGrouping<string, MediaFile> group) =>
        group.Key;

    private static string MostPopulousGenre(IGrouping<string, MediaFile> group) =>
        group
            .Select(f => f.Genres.First())
            .GroupBy(genre => genre)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .First() // Keep only the most populous genre.
            .Trim();

    public void Start(
        IReadOnlyCollection<MediaFile> mediaFiles,
        DirectoryInfo workingDirectory,
        Settings settings,
        IPrinter printer)
    {
        if (string.IsNullOrWhiteSpace(settings.ArtistGenreCsvFilePath))
        {
            printer.Error("You must specify a .csv file path for artist and genre data in your settings file under the 'artistGenreCsvFilePath' key.");
            return;
        }

        Watch watch = new();

        Dictionary<string, string> existingGenres;
        if (settings.ResetSavedArtistGenres)
        {
            printer.Print("Will delete any existing genre data.");
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

        var relevantFiles = mediaFiles.Where(HasGenresAndArtists);
        if (!relevantFiles.Any())
        {
            printer.Error("There are no media files with an artist and genre to process.");
            return;
        }

        var latestGenres =
            relevantFiles
                .GroupBy(FirstArtistTrimmed)
                .ToImmutableSortedDictionary(ArtistName, MostPopulousGenre);

        printer.Print($"Found {latestGenres.Count:#,##0} unique artists with genres.");

        var mergedGenres =
            latestGenres
                .Concat(existingGenres)
                .GroupBy(g => g.Key)
                .ToImmutableDictionary(
                    g =>g.Key,
                    g => g.First().Value); // Prioritize the first dictionary's values.

        WriteSummary(existingGenres.Count, mergedGenres.Count, printer);

        Result writeResult = GenreService.Write(settings.ArtistGenreCsvFilePath, mergedGenres);

        if (writeResult.IsSuccess)
            printer.Success($"Genres written to \"{settings.ArtistGenreCsvFilePath}\" in {watch.ElapsedFriendly}.");
        else
            printer.Error(writeResult.Errors.First().Message);

        // TODO: When resetting data, no need for the beforeVsAfter statement, so remove it.
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
