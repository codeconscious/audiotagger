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

        var artistsWithGenres = mediaFiles
            .Where(f => f.Genres.Any() && f.Artists.Any())
            .GroupBy(f => f.ArtistsCombined, f => f.Genres)
            .ToImmutableSortedDictionary(
                f => f.Key,
                f => f.First() // TODO: Get their most populous genre
            );

        printer.Print($"Found {artistsWithGenres.Count:#,##0} artists with genres.");

        settings.ArtistGenres ??= new();
        foreach (var pair in artistsWithGenres)
        {
            settings.ArtistGenres[pair.Key] = pair.Value[0];
        }

        if (SettingsService.WriteSettingsToFile(settings, printer))
            printer.Print("Saved artists and genres to the settings file.");
        else
            printer.Print("An error occurred during the process.");
    }
}
