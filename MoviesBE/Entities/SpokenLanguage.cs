using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class SpokenLanguage
{
    [JsonPropertyName("english_name")]
    public string? EnglishName { get; init; }
    [JsonPropertyName("iso_639_1")]
    public string? Iso6391 { get; init; }
    public string? Name { get; init; }
}