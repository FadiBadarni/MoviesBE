using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class MovieVideo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("iso_639_1")]
    public string Iso6391 { get; set; }

    [JsonPropertyName("iso_3166_1")]
    public string Iso31661 { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("site")]
    public string Site { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}