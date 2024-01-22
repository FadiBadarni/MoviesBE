using System.Net.Http.Headers;
using MoviesBE.Data;
using Newtonsoft.Json;

namespace MoviesBE.Services;

public class TmdbService
{
    private readonly string _apiReadAccessToken;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TmdbService> _logger;

    public TmdbService(HttpClient httpClient, IConfiguration configuration, ILogger<TmdbService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiReadAccessToken = configuration["TMDB:ApiReadAccessToken"] ??
                              throw new InvalidOperationException("API Read Access Token is not configured.");
        _baseUrl = configuration["TMDB:BaseUrl"] ?? throw new InvalidOperationException("Base URL is not configured.");
    }

    public async Task<Movie> GetMovieAsync(int movieId)
    {
        var requestUri = $"{_baseUrl}movie/{movieId}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri)
        {
            Headers =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
                Authorization = new AuthenticationHeaderValue("Bearer", _apiReadAccessToken)
            }
        };

        try
        {
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var movie = JsonConvert.DeserializeObject<Movie>(responseContent);

            if (movie == null) throw new InvalidOperationException("Deserialization of the movie data returned null.");

            return movie;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching movie data for movie ID {MovieId}", movieId);
            throw;
        }
    }
}