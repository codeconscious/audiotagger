using System.Text.Json.Serialization;

namespace AudioTagger.Library.UserSettings;

public sealed record Settings
{
    [JsonPropertyName("duplicates")]
    public Duplicates Duplicates { get; init; } = new();

    [JsonPropertyName("tagging")]
    public Tagging? Tagging { get; init; }

    [JsonPropertyName("renaming")]
    public Renaming? Renaming { get; init; }

    [JsonPropertyName("artistGenreCsvFilePath")]
    public string? ArtistGenreCsvFilePath { get; init; }

    [JsonPropertyName("resetSavedArtistGenres")]
    public bool ResetSavedArtistGenres { get; init; }

    [JsonPropertyName("tagCacheFilePath")]
    public string? TagCacheFilePath { get; init; }
}

/// <summary>
/// Settings related to finding duplicate tracks via their tag data.
/// </summary>
public sealed record Duplicates
{
    /// <summary>
    /// A collection of substrings that should be ignored when searching for
    /// duplicate artist names. For example, if "The " is added, then
    /// "The Beatles" and "Beatles" would be recognized as the same artist.
    /// </summary>
    [JsonPropertyName("titleReplacements")]
    public ImmutableList<string>? TitleReplacements { get; init; }

    /// <summary>
    /// A collection of substrings that should be ignored when searching for
    /// duplicate track titles. For example, if "(Remix)" is added, then
    /// "TrackTitle" and "TrackTitle (Remix)" would be considered identical.
    /// </summary>
    [JsonPropertyName("artistReplacements")]
    public ImmutableList<string>? ArtistReplacements { get; init; }

    /// <summary>
    /// When creating a playlist file, search for this substring in
    /// the duplicate files' paths (with the intention of replacing it
    /// with the string specified in the `PathReplaceWith` property).
    /// </summary>
    [JsonPropertyName("pathSearchFor")]
    public string? PathSearchFor { get; init; }

    /// <summary>
    /// When creating a playlist of duplicate tracks, replace the matched
    /// path substring from the `PathSearchFor` property with this text.
    /// </summary>
    [JsonPropertyName("pathReplaceWith")]
    public string? PathReplaceWith { get; init; }

    /// <summary>
    /// The directory to which the playlist of duplicates should be saved.
    /// </summary>
    [JsonPropertyName("savePlaylistDirectory")]
    public string? SavePlaylistDirectory { get; init; }

    /// <summary>
    /// Track metadata that should be ignored when searching for duplicates.
    /// </summary>
    [JsonPropertyName("exclusions")]
    public ImmutableList<ExclusionPair>? Exclusions { get; init; }
}

/// <summary>
/// An artist, track title, or combinations of both that should be ignored
/// when searching for duplicate tracks.
/// </summary>
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
    public ImmutableList<string>? RegexPatterns { get; set; }
}

public sealed record Renaming
{
    [JsonPropertyName("patterns")]
    public ImmutableList<string>? Patterns { get; }

    [JsonPropertyName("useAlbumDirectories")]
    public bool UseAlbumDirectories { get; } = false;

    [JsonPropertyName("ignoredDirectories")]
    public ImmutableList<string>? IgnoredDirectories { get; }
}
