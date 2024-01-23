using System.Text.Json.Serialization;

namespace MoviesBE.Data;

public class MovieListResult
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("results")]
    public List<Movie>? Results { get; init; }
}