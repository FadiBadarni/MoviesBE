using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class MovieBackdrop
{
    [JsonPropertyName("file_path")]
    public string? FilePath { get; init; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; init; }

    [JsonPropertyName("aspect_ratio")]
    public double AspectRatio { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }
}