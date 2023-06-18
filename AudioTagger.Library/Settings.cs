using System.Text.Json.Serialization;

namespace AudioTagger;

public record Settings
{
    [JsonPropertyName("duplicates")]
    public Duplicates? Duplicates { get; set; }
}

public record Duplicates
{
    [JsonPropertyName("titleReplacements")]
    public ImmutableList<string>? TitleReplacements { get; set; }
}
