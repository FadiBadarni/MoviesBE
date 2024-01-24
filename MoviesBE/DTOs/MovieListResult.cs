using System.Text.Json.Serialization;
using MoviesBE.Entities;

namespace MoviesBE.DTOs;

public class MovieListResult
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("results")]
    public List<Movie>? Results { get; init; }
}