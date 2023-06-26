using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace AudioTagger.Console;

public sealed class GenreExtractor : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings? settings = null)
    {
        if (settings is null)
            throw new InvalidOperationException("Settings cannot be null");

        if (!mediaFiles.Any())
        {
            printer.Print("There are no files to work on. Cancelling...");
            return;
        }

        var artistsWithGenres = mediaFiles
            .Where(f => f.Genres.Any() && f.Artists.Any())
            .GroupBy(f => f.Artists[0], f => f.Genres)
            .ToImmutableDictionary(
                f => f.Key,
                f => f.First() // TODO: Get their most populous genre
            );

        printer.Print($"Found {artistsWithGenres.Count} artists with genres.");

        settings.ArtistGenres ??= new();
        foreach (var pair in artistsWithGenres)
        {
            settings.ArtistGenres[pair.Key] = pair.Value[0];
        }

        SettingsService.WriteSettingsFile(settings);
        printer.Print("Wrote to the settings file!");
    }
}
