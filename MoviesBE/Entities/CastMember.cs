using System.Text.Json.Serialization;

namespace MoviesBE.Entities;

public class CastMember
{
    [JsonPropertyName("adult")]
    public bool Adult { get; init; }

    [JsonPropertyName("gender")]
    public int Gender { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("known_for_department")]
    public string? KnownForDepartment { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("popularity")]
    public double Popularity { get; init; }

    [JsonPropertyName("profile_path")]
    public string? ProfilePath { get; init; }

    [JsonPropertyName("cast_id")]
    public int CastId { get; init; }

    [JsonPropertyName("character")]
    public string? Character { get; init; }

    [JsonPropertyName("credit_id")]
    public string? CreditId { get; init; }

    [JsonPropertyName("order")]
    public int Order { get; init; }
}