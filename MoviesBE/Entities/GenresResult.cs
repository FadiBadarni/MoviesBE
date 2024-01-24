using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class GenresResult
{
    [JsonPropertyName("genres")]
    public List<Genre>? Genres { get; init; }
}