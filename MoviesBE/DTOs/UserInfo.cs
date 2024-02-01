using System.Text.Json.Serialization;

namespace MoviesBE.DTOs;

public class UserInfo
{
    [JsonPropertyName("sub")]
    public string Sub { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("picture")]
    public string Picture { get; set; }

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("locale")]
    public string Locale { get; set; }
}