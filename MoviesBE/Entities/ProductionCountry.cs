using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class ProductionCountry
{
    [JsonPropertyName("iso_3166_1")]
    public string? Iso31661 { get; init; }
    public string? Name { get; init; }
}