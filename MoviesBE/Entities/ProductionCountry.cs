using System.Text.Json.Serialization;

namespace MoviesBE.Data;

public class ProductionCountry
{
    [JsonPropertyName("iso_3166_1")]
    public string? Iso31661 { get; init; }
    public string? Name { get; init; }
}