using System.Text.Json.Serialization;

namespace AudioTagger;

public record Settings
{
    [JsonPropertyName("duplicates")]
    public Duplicates Duplicates { get; set; } = new();

    [JsonPropertyName("tagging")]
    public Tagging Tagging { get; set; } = new();
}

public record Duplicates
{
    [JsonPropertyName("titleReplacements")]
    public ImmutableList<string>? TitleReplacements { get; set; }
}

public record Tagging
{
    [JsonPropertyName("regexPatterns")]
    public ImmutableList<string>? RegexPatterns { get; set; }
}
