using System.Text.Json.Serialization;

namespace MoviesBE.Data;

public class GenresResult
{
    [JsonPropertyName("genres")]
    public List<Movie.Genre>? Genres { get; init; }
}