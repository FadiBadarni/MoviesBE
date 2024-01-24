using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class MovieCollection
{
    public int Id { get; init; }
    public string? Name { get; init; }
    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; init; }
    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; init; }
}