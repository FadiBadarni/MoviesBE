using System.Net.Http.Headers;
using System.Text.Json;
using MoviesBE.Exceptions;

namespace MoviesBE.Services;

public class HttpService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _apiReadAccessToken;
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpService> _logger;

    public HttpService(HttpClient httpClient, IConfiguration configuration, ILogger<HttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiReadAccessToken = configuration["TMDB:ApiReadAccessToken"] ??
                              throw new InvalidOperationException("API Read Access Token is not configured.");
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        try
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("HTTP request to {Url} failed: {Message}", request.RequestUri, ex.Message);
            throw new ExternalServiceException("An error occurred while making an HTTP request.", ex);
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage(method, requestUri)
        {
            Headers =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
                Authorization = new AuthenticationHeaderValue("Bearer", _apiReadAccessToken)
            }
        };
        return request;
    }

    public async Task<T> SendAndDeserializeAsync<T>(string requestUri)
    {
        var request = CreateRequest(HttpMethod.Get, requestUri);
        using var response = await SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, JsonSerializerOptions);
    }
}