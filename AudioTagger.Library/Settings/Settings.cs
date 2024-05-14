using System.Text.Json.Serialization;

namespace AudioTagger.Library.Settings;

public sealed record Settings
{
    [JsonPropertyName("duplicates")]
    public Duplicates Duplicates { get; set; } = new();

    [JsonPropertyName("tagging")]
    public Tagging? Tagging { get; set; }

    [JsonPropertyName("renaming")]
    public Renaming? Renaming { get; set; }

    [JsonPropertyName("artistGenreCsvFilePath")]
    public string? ArtistGenreCsvFilePath { get; set; } = null;

    [JsonPropertyName("resetSavedArtistGenres")]
    public bool ResetSavedArtistGenres { get; set; } = false;

    [JsonPropertyName("tagLibraryFilePath")]
    public string? TagLibraryFilePath { get; set; }
}

public sealed record Duplicates
{
    [JsonPropertyName("titleReplacements")]
    public ImmutableList<string>? TitleReplacements { get; set; }

    [JsonPropertyName("pathSearchFor")]
    public string? PathSearchFor { get; set; }

    [JsonPropertyName("pathReplaceWith")]
    public string? PathReplaceWith { get; set; }

    [JsonPropertyName("savePlaylistDirectory")]
    public string? SavePlaylistDirectory { get; set; }

    [JsonPropertyName("exclusions")]
    public ImmutableList<ExclusionPair>? Exclusions { get; set; }
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
    public ImmutableList<string>? RegexPatterns { get; set; }
}

public sealed record Renaming
{
    [JsonPropertyName("patterns")]
    public ImmutableList<string>? Patterns { get; set; }

    [JsonPropertyName("useAlbumDirectories")]
    public bool UseAlbumDirectories { get; set; } = false;

    [JsonPropertyName("ignoredDirectories")]
    public ImmutableList<string>? IgnoredDirectories { get; set; }
}
