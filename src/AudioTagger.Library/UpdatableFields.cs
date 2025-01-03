using System.Text.RegularExpressions;
using System.Globalization;
using AudioTagger.Library.MediaFiles;

namespace AudioTagger.Library;

public sealed class UpdatableFields
{
    public string[]? AlbumArtists { get; }
    public string[]? Artists { get; }
    public string? Title { get; }
    public string? Album { get; }
    public uint? Year { get; }
    public uint? TrackNo { get; }
    public string[]? Genres { get; }

    private byte Count { get; }

    /// <summary>
    /// Constructor that reads matched regex group names and
    /// maps the data to the correct tag name property.
    /// </summary>
    /// <param name="matchedGroups"></param>
    public UpdatableFields(
        IEnumerable<Group> matchedGroups,
        IDictionary<string, string> artistsWithGenres)
    {
        ArgumentNullException.ThrowIfNull(matchedGroups);

        foreach (Group element in matchedGroups)
        {
            if (element.Name == "title")
            {
                // TODO: Relocate the replacements.
                Title = element.Value.Trim().Normalize()
                                     .Replace("___", "　")
                                     .Replace("__", " ");
                Count++;
            }
            else if (element.Name == "albumArtists")
            {
                AlbumArtists = element.Value
                                      .Replace("___", "　")
                                      .Replace("__", " ")
                                      .Split([";"],
                                             StringSplitOptions.RemoveEmptyEntries |
                                             StringSplitOptions.TrimEntries)
                                      .Select(a => a.Normalize())
                                      .ToArray();
                Count++;
            }
            else if (element.Name == "artists")
            {
                Artists = element.Value.Replace("___", "　")
                                       .Replace("__", " ")
                                       .Split([";"],
                                              StringSplitOptions.RemoveEmptyEntries |
                                              StringSplitOptions.TrimEntries)
                                       .Select(a => a.Normalize())
                                       .ToArray();
                Count++;
            }
            else if (element.Name == "album")
            {
                Album = element.Value.Trim().Normalize()
                                     .Replace("___", "　")
                                     .Replace("__", " ");
                Count++;
            }
            else if (element.Name == "genres")
            {
                Genres = element.Value.Replace("___", "　")
                                      .Replace("__", " ")
                                      .Split([";"],
                                             StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                      .Select(g => g.Normalize())
                                      .ToArray();
                Count++;
            }
            else if (element.Name == "year")
            {
                Year = uint.TryParse(element.Value, out var parsed) ? parsed : 0;
                Count++;
            }
            else if (element.Name == "trackNo")
            {
                TrackNo = uint.TryParse(element.Value, out var parsed) ? parsed : null;
                Count++;
            }
        }

        // If no genre was manually passed in, check the settings for a registered one.
        if (Genres?.Any() != true &&
            Artists?.Any() == true &&
            artistsWithGenres.Any())
        {
            if (artistsWithGenres.Any() &&
                artistsWithGenres.ContainsKey(Artists[0]))
            {
                Genres = [artistsWithGenres[Artists[0]]];
            }
        }
    }

    public UpdatableFields(string tagField, dynamic newValue)
    {
        ArgumentNullException.ThrowIfNull(tagField);
        ArgumentNullException.ThrowIfNull(newValue);

        if (tagField.Equals("year", StringComparison.OrdinalIgnoreCase) &&
            newValue is int newYear)
        {
            Year = (uint)newYear;
            Count++;
        }
        else if (tagField.Equals("genre", StringComparison.OrdinalIgnoreCase) &&
                 newValue is string newGenre)
        {
            Genres = [newGenre];
            Count++;
        }
        else if (tagField.Equals("genre", StringComparison.OrdinalIgnoreCase) &&
                 newValue is string[] newGenres)
        {
            Genres = newGenres;
            Count++;
        }
    }

    public Dictionary<string, string> GetUpdateKeyValuePairs(MediaFile fileData)
    {
        var updateOutput = new Dictionary<string, string>();

        if (AlbumArtists?.All(a => fileData.AlbumArtists.Contains(a)) == false)
        {
            updateOutput.Add("Album Artists", string.Join("; ", AlbumArtists));
        }

        if (Artists?.All(a => fileData.Artists.Contains(a)) == false)
        {
            updateOutput.Add("Artists", string.Join("; ", Artists));
        }

        if (Title != null && Title != fileData.Title)
        {
            updateOutput.Add("Title", Title);
        }

        if (Album != null && Album != fileData.Album)
        {
            updateOutput.Add("Album", Album);
        }

        if (Year != null && Year != fileData.Year)
        {
            updateOutput.Add("Year", Year.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (TrackNo != null && TrackNo != fileData.TrackNo)
        {
            updateOutput.Add("Track", TrackNo.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (Genres?.All(a => fileData.Genres.Contains(a)) == false)
        {
            int genreCount = Genres.Length;

            updateOutput.Add(
                "Genres",
                string.Join(
                    "; ", Genres) +
                    (genreCount > 1
                        ? $" ({genreCount})"
                        : string.Empty));
        }

        return updateOutput;
    }
}
