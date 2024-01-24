using System.Text.Json.Serialization;

namespace MoviesBE.Data;

public class GenresResult
{
    [JsonPropertyName("genres")]
    public List<Genre>? Genres { get; init; }
}