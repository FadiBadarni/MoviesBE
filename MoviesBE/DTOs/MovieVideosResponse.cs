using System.Text.Json.Serialization;
using MoviesBE.Entities;

namespace MoviesBE.DTOs;

public class MovieVideosResponse
{
    [JsonPropertyName("results")]
    public List<MovieVideo> Videos { get; set; }
}