using System.Diagnostics;
using AudioTagger.Library.Genres;

namespace AudioTagger.Console;

public sealed class GenreExtractor : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      Settings settings,
                      IPrinter printer)
    {
        // TODO: If possible, front-load this so that it occurs before all the files are read.
        // Reading all the files can take several minutes in some situations.
        if (string.IsNullOrWhiteSpace(settings.ArtistGenreCsvFilePath))
        {
            printer.Error("You must specify a comma-separated file (.csv) containing artist and genre data in your settings file under the 'artistGenreCsvFilePath' key.");
            return;
        }
        printer.Print($"Will save to \"{settings.ArtistGenreCsvFilePath}\".");

        Stopwatch stopwatch = new();
        stopwatch.Start();

        if (File.Exists(settings.ArtistGenreCsvFilePath))
        {
            printer.Warning("Overwriting the existing genre file.");
        }

        ImmutableSortedDictionary<string, string> artistsWithGenres = mediaFiles
            .Where(f => f.Genres.Any() && f.Artists.Any())
            .GroupBy(f =>
                f.Artists[0], // Only the first artist (though it might be nice to process all someday)
                f => f.Genres.GroupBy(g => g) // Get most populous...
                             .OrderByDescending(grp => grp.Count()) // ...and keep them at the top.
                             .Select(grp => grp.Key)
                             .First()) // Keep only the most single most populous genre.
            .ToImmutableSortedDictionary(
                f => f.Key,
                f => f.First()
             );

        printer.Print($"Found {artistsWithGenres.Count:#,##0} unique artists with genres.");

        Result writeResult = GenreService.Write(settings.ArtistGenreCsvFilePath, artistsWithGenres);

        double elapsedMs = TimeSpan.FromTicks(stopwatch.ElapsedTicks).TotalMilliseconds;
        if (writeResult.IsSuccess)
            printer.Success($"File written successfully after {elapsedMs:#,##0}ms.");
        else
            printer.Error(writeResult.Errors.First().Message);
    }
}
