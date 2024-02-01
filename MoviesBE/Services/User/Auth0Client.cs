using System.Net.Http.Headers;
using System.Text.Json;
using MoviesBE.DTOs;

namespace MoviesBE.Services.User;

public class Auth0Client
{
    private readonly HttpClient _httpClient;
    private readonly string _userInfoEndpoint;

    public Auth0Client(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _userInfoEndpoint = configuration["Auth0:UserInfoEndpoint"];
    }

    public async Task<UserInfo> GetUserInfoAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _userInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfo>(content);
            return userInfo ?? throw new InvalidOperationException("User info deserialization failed.");
        }

        throw new HttpRequestException($"Error retrieving user info from Auth0. Status Code: {response.StatusCode}");
    }
}