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

    [JsonPropertyName("artistGenresFilePath")]
    public string? ArtistGenresFilePath { get; set; } = null;

    [JsonPropertyName("renameUseAlbumFolders")]
    public bool RenameUseAlbumFolders { get; set; } = false;
}

public sealed record Duplicates
{
    [JsonPropertyName("titleReplacements")]
    public ImmutableList<string>? TitleReplacements { get; set; }
}

public sealed record Tagging
{
    [JsonPropertyName("regexPatterns")]
    public ImmutableList<string>? RegexPatterns { get; set; }
}
