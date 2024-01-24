using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class ProductionCompany
{
    public int Id { get; init; }
    [JsonPropertyName("logo_path")]
    public string? LogoPath { get; init; }
    public string? Name { get; init; }
    [JsonPropertyName("origin_country")]
    public string? OriginCountry { get; init; }
}