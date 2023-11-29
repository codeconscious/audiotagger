using AudioTagger.Library.Genres;

namespace AudioTagger.Console;

public sealed class TagUpdaterGenreOnly : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> mediaFiles,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        if (settings.ArtistGenresFilePath is null)
        {
            printer.Error("No artist genre file has been specified in the settings, so cannot continue.");
            return;
        }


        var readResult = GenreService.Read(settings.ArtistGenresFilePath);
        if (readResult.IsFailed)
        {
            printer.Error(readResult.Errors.First().Message);
            return;
        }

        List<string> errorFiles = [];
        foreach (MediaFile mediaFile in mediaFiles)
        {
            try
            {
                UpdateGenreTag(mediaFile, readResult.Value, printer);
            }
            catch (Exception ex)
            {
                printer.Error($"Error updating {mediaFile.FileNameOnly}: {ex.Message}");
                errorFiles.Add(mediaFile.FileNameOnly);
                continue;
            }
        }

        if (errorFiles.Any())
        {
            printer.Print("Files with errors:");
            errorFiles.ForEach(f => printer.Print("• " + f));
        }
    }

    /// <summary>
    /// Make proposed tag updates to the specified file if the user agrees.
    /// </summary>
    /// <returns>A bool indicating whether the following file should be processed.</returns>
    private static void UpdateGenreTag(MediaFile mediaFile,
                                       Dictionary<string, string> artistsWithGenres,
                                       IPrinter printer)
    {
        string? artistName = mediaFile.AlbumArtists.FirstOrDefault()
                             ?? mediaFile.Artists.FirstOrDefault();

        if (artistName is null)
        {
            printer.Print($"No artist name found, so skipping file \"{mediaFile.FileNameOnly}\".");
            return;
        }

        if (!artistsWithGenres.ContainsKey(artistName))
        {
            printer.Print($"No registered genre found for artist \"{artistName}\", so skipping \"{mediaFile.FileNameOnly}\".");
            return;
        }

        if (mediaFile.Genres.FirstOrDefault() == artistsWithGenres[artistName])
        {
            printer.Print($"Genre is correct, so skipping \"{mediaFile.FileNameOnly}\".");
            return;
        }

        const string updateType = "genre";
        UpdatableFields updateableFields = new(updateType, artistsWithGenres[artistName]);
        Dictionary<string, string> proposedUpdates = updateableFields.GetUpdateKeyValuePairs(mediaFile);
        if (!proposedUpdates.Any())
        {
            printer.Print($"No {updateType} updates needed for \"{mediaFile.FileNameOnly}\".");
        }

        // printer.PrintTagDataToTable(mediaFile, proposedUpdates, false);

        // Make the tag updates
        try
        {
            UpdateFileTags(mediaFile, updateableFields);
            mediaFile.SaveUpdates();
        }
        catch (TagLib.CorruptFileException ex)
        {
            printer.Error("Saving failed: " + ex.Message);
            return;
        }

        printer.Print($"Added \"{proposedUpdates["Genres"]}\" to '{mediaFile.FileNameOnly}'", 0, 0, fgColor: ConsoleColor.Green);
        return;
    }

    /// <summary>
    /// Update file tags where they differ from parsed filename data.
    /// </summary>
    /// <param name="fileData"></param>
    /// <param name="updateableFields"></param>
    private static void UpdateFileTags(MediaFile fileData, UpdatableFields updateableFields)
    {
        if (updateableFields.Title != null && updateableFields.Title != fileData.Title)
        {
            fileData.Title = updateableFields.Title;
        }

        if (updateableFields.Album != null && updateableFields.Album != fileData.Album)
        {
            fileData.Album = updateableFields.Album;
        }

        if (updateableFields.Artists?.All(a => fileData.Artists.Contains(a)) == false)
        {
            fileData.Artists = updateableFields.Artists;
        }

        if (updateableFields.Year != null && updateableFields.Year != fileData.Year)
        {
            fileData.Year = updateableFields.Year.Value;
        }

        if (updateableFields.TrackNo != null && updateableFields.TrackNo != fileData.TrackNo)
        {
            fileData.TrackNo = updateableFields.TrackNo.Value;
        }

        if (updateableFields.Genres?.All(a => fileData.Genres.Contains(a)) == false)
        {
            fileData.Genres = updateableFields.Genres;
        }
    }
}
