using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class Credits
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("cast")]
    public List<CastMember>? Cast { get; init; }

    [JsonPropertyName("crew")]
    public List<CrewMember>? Crew { get; init; }
}