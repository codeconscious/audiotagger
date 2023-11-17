namespace AudioTagger.Console;

public sealed class GenreExtractor : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        ImmutableList<(string Artist, string Genre)> sortedArtistsWithGenres = mediaFiles
            .Where(f => f.Genres.Any() && f.Artists.Any())
            .GroupBy(f =>
                f.Artists[0], // Would be nice to split them
                f => f.Genres.GroupBy(g => g) // Get most populous
                             .OrderByDescending(grp => grp.Count())
                             .Select(grp=>grp.Key)
                             .First())
            .ToImmutableSortedDictionary(
                f => f.Key,
                f => f.First()
            )
            .Select(pair => (
                Artist: pair.Key,
                Genre: pair.Value
            ))
            .ToImmutableList();

        printer.Print($"Found {sortedArtistsWithGenres.Count:#,##0} unique artists with genres.");

        settings.ArtistGenres ??= [];
        foreach ((string artist, string genre) in sortedArtistsWithGenres)
        {
            settings.ArtistGenres[artist] = genre;
        }

        if (SettingsService.Write(settings, printer))
            printer.Print("Saved artists and genres to the settings file.");
        else
            printer.Error("An error occurred during the process.");
    }
}
