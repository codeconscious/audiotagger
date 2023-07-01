namespace AudioTagger.Console;

public sealed class GenreExtractor : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings? settings = null) // # TODO: No need for nullability
    {
        if (settings is null)
            throw new InvalidOperationException("Settings cannot be null.");

        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        var sortedArtistsWithGenres = mediaFiles
            .Where(f => f.Genres.Any() && f.Artists.Any())
            .GroupBy(f =>
                f.Artists[0], // Would be nice to split them
                f => f.Genres.GroupBy(g => g)
                             .OrderByDescending(grp => grp.Count())
                             .Select(grp=>grp.Key)
                             .First())
            .ToImmutableSortedDictionary(
                f => f.Key,
                f => f.First()
            );

        printer.Print($"Found {sortedArtistsWithGenres.Count:#,##0} unique artists with genres.");

        settings.ArtistGenres ??= new();
        foreach (var pair in sortedArtistsWithGenres)
        {
            settings.ArtistGenres[pair.Key] = pair.Value;
        }

        if (SettingsService.WriteSettingsToFile(settings, printer))
            printer.Print("Saved artists and genres to the settings file.");
        else
            printer.Error("An error occurred during the process.");
    }
}
