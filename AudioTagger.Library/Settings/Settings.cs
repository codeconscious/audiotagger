using System.Text.Json.Serialization;

namespace AudioTagger.Library.Settings;

public sealed record Settings
{
    public static Settings CreateEmpty()
    {
        return new Settings()
            {
                Tagging = new(),
                Duplicates = new(),
                Renaming = new()
            };
    }

    [JsonPropertyName("tagging")]
    public Tagging? Tagging { get; init; }

    [JsonPropertyName("duplicates")]
    public Duplicates Duplicates { get; init; } = new();

    [JsonPropertyName("renaming")]
    public Renaming? Renaming { get; init; }

    [JsonPropertyName("artistGenreCsvFilePath")]
    public string? ArtistGenreCsvFilePath { get; init; } = null;

    [JsonPropertyName("resetSavedArtistGenres")]
    public bool ResetSavedArtistGenres { get; init; } = false;

    [JsonPropertyName("tagCacheFilePath")]
    public string? TagCacheFilePath { get; init; }
}

/// <summary>
/// Settings used for finding audio files with identical or near-identical
/// tags. It provides the ability to ignore certain facets of tag data
/// for a more custom search.
/// </summary>
public sealed record Duplicates
{
    /// <summary>
    /// An optional collection of strings, each of which, when found,
    /// should be ignored in the track title for purposes of finding
    /// duplicates. (Operations are done in memory only, and the actual
    /// title tag is not modified.)
    /// </summary>
    [JsonPropertyName("titleReplacements")]
    public ImmutableList<string>? TitleReplacements { get; init; }

    /// <summary>
    /// Before saving a playlist file, search the full paths of the
    /// files to be included for the following text (which will be
    /// replaced).
    /// </summary>
    /// <remarks>Used in conjunction with `PathReplaceWith`.</remarks>
    [JsonPropertyName("pathSearchFor")]
    public string? PathSearchFor { get; init; }

    /// <summary>
    /// Before saving a playlist file, replace the string identified in
    /// `PathSearchFor` and replace all occurrences with this string,
    /// if specified.
    /// </summary>
    /// <remarks>Used in conjunction with `PathSearchFor`.</remarks>
    [JsonPropertyName("pathReplaceWith")]
    public string? PathReplaceWith { get; init; }

    /// <summary>
    /// The full directory path to which the playlist of duplicates
    /// should be saved. (Its filename will be automatically generated.)
    /// </summary>
    [JsonPropertyName("savePlaylistDirectory")]
    public string? SavePlaylistDirectory { get; init; }

    /// <summary>
    /// An optional collection of track artists and/or titles that
    /// should be excluded from duplicate searches.
    /// </summary>
    [JsonPropertyName("exclusions")]
    public ImmutableList<ExclusionPair>? Exclusions { get; init; }
}

public sealed record ExclusionPair
{
    [JsonPropertyName("artist")]
    public string? Artist { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }
}

public sealed record Tagging
{
    [JsonPropertyName("regexPatterns")]
    public ImmutableList<string>? RegexPatterns { get; init; }
}

public sealed record Renaming
{
    [JsonPropertyName("patterns")]
    public ImmutableList<string>? Patterns { get; init; }

    [JsonPropertyName("useAlbumDirectories")]
    public bool UseAlbumDirectories { get; init; } = false;

    [JsonPropertyName("ignoredDirectories")]
    public ImmutableList<string>? IgnoredDirectories { get; init; }
}
