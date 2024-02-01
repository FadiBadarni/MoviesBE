using System.Text.Json.Serialization;

namespace MoviesBE.DTOs;

public class UserInfo
{
    [JsonPropertyName("sub")]
    public string? Sub { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("picture")]
    public string? Picture { get; init; }

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }
}