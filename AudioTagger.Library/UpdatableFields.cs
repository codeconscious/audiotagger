using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AudioTagger;

public class UpdatableFields
{
    public string[]? AlbumArtists { get; }
    public string[]? Artists { get; }
    public string? Title { get; }
    public string? Album { get; }
    public uint? Year { get; }
    public uint? TrackNo { get; }
    public string[]? Genres { get; }

    public byte Count { get; }

    /// <summary>
    /// Constructor that reads matched regex group names and
    /// maps the data to the correct tag name property.
    /// </summary>
    /// <param name="matchedGroups"></param>
    public UpdatableFields(IEnumerable<Group> matchedGroups)
    {
        ArgumentNullException.ThrowIfNull(matchedGroups);

        foreach (var element in matchedGroups)
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
                                      .Split(new[] { ";" },
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
                                       .Split(new[] { ";" },
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
                                      .Split(new[] { ";" },
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
    }

    public UpdatableFields(string tagField, dynamic newValue)
    {
        ArgumentNullException.ThrowIfNull(tagField);
        ArgumentNullException.ThrowIfNull(newValue);

        if (tagField.Equals("year", StringComparison.OrdinalIgnoreCase) &&
            newValue is int newYear)
        {
            Year = (uint) newYear;
            Count++;
        }
    }

    // TODO: Delete if not used.
    /*
    public IList<OutputLine> GetUpdateOutput(MediaFile fileData)
    {
        var updateOutput = new List<OutputLine>();
        const string prependLineWith = "";

        if (Artists?.All(a => fileData.Artists.Contains(a)) == false)
        {
            updateOutput.Add(
                OutputLine.TagDataWithHeader(
                    "Artists",
                    string.Join("; ", Artists),
                    prependLineWith));
        }

        if (Title != null && Title != fileData.Title)
        {
            updateOutput.Add(
                OutputLine.TagDataWithHeader(
                    "Title",
                    Title,
                    prependLineWith));
        }

        if (Album != null && Album != fileData.Album)
        {
            updateOutput.Add(
                OutputLine.TagDataWithHeader(
                    "Album",
                    Album,
                    prependLineWith));
        }

        if (Year != null && Year != fileData.Year)
        {
            updateOutput.Add(
                OutputLine.TagDataWithHeader(
                    "Year",
                    Year.Value.ToString(CultureInfo.InvariantCulture),
                    prependLineWith));
        }

        if (TrackNo != null && TrackNo != fileData.TrackNo)
        {
            updateOutput.Add(
                OutputLine.TagDataWithHeader(
                    "Track",
                    TrackNo.Value.ToString(CultureInfo.InvariantCulture),
                    prependLineWith));
        }

        if (Genres?.All(a => fileData.Genres.Contains(a)) == false)
        {
            var genreCount = Genres.Length;

            updateOutput.Add(
                OutputLine.TagDataWithHeader(
                    "Genres",
                    string.Join("; ", Genres) + (genreCount > 1 ? $" ({genreCount})" : ""),
                    prependLineWith));
        }

        return updateOutput;
    }
    */

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
            var genreCount = Genres.Length;

            updateOutput.Add(
                "Genres",
                string.Join(
                    "; ", Genres) +
                    (genreCount > 1
                        ? $" ({genreCount})"
                        : ""));
        }

        return updateOutput;
    }
}