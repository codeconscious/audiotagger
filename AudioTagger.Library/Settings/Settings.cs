using System.Text.Json.Serialization;

namespace AudioTagger.Library.Settings;

public sealed record Settings
{
    [JsonPropertyName("duplicates")]
    public Duplicates Duplicates { get; set; } = new();

    [JsonPropertyName("tagging")]
    public Tagging? Tagging { get; set; }

    [JsonPropertyName("renamePatterns")]
    public ImmutableList<string>? RenamePatterns { get; set; }

    [JsonPropertyName("artistGenreCsvFilePath")]
    public string? ArtistGenreCsvFilePath { get; set; } = null;

    [JsonPropertyName("renameUseAlbumFolders")]
    public bool RenameUseAlbumFolders { get; set; } = false;

    [JsonPropertyName("resetSavedArtistGenres")]
    public bool ResetSavedArtistGenres { get; set; } = false;
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
}

public sealed record Tagging
{
    [JsonPropertyName("regexPatterns")]
    public ImmutableList<string>? RegexPatterns { get; set; }
}
