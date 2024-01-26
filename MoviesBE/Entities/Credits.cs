using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class Credits
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("cast")]
    public List<CastMember>? Cast { get; set; }

    [JsonPropertyName("crew")]
    public List<CrewMember>? Crew { get; set; }
}