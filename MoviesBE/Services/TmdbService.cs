using System.Net.Http.Headers;
using System.Text.Json;
using MoviesBE.Data;

namespace MoviesBE.Services;

public class TmdbService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _apiReadAccessToken;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TmdbService> _logger;
    private readonly Neo4JService _neo4JService;

    public TmdbService(HttpClient httpClient, IConfiguration configuration, ILogger<TmdbService> logger,
        Neo4JService neo4JService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiReadAccessToken = configuration["TMDB:ApiReadAccessToken"] ??
                              throw new InvalidOperationException("API Read Access Token is not configured.");
        _baseUrl = configuration["TMDB:BaseUrl"] ?? throw new InvalidOperationException("Base URL is not configured.");
        _neo4JService = neo4JService ?? throw new ArgumentNullException(nameof(neo4JService));
    }

    public async Task<ServiceResult<Movie>> GetMovieAsync(int movieId)
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
            var movie = JsonSerializer.Deserialize<Movie>(responseContent, JsonSerializerOptions);

            if (movie == null)
            {
                return new ServiceResult<Movie>(null, false, "Movie not found");
            }

            await _neo4JService.SaveMovieAsync(movie);
            return new ServiceResult<Movie>(movie, true, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching movie data for movie ID {MovieId}", movieId);
            return new ServiceResult<Movie>(null, false, "Error fetching data from API");
        }
    }

    public async Task<List<Movie>> GetPopularMoviesAsync()
    {
        // First fetch the full list of genres
        var genresLookup = await GetGenresAsync();

        var requestUri = $"{_baseUrl}movie/popular";
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
            var movieListResult = JsonSerializer.Deserialize<MovieListResult>(responseContent, JsonSerializerOptions);

            if (movieListResult?.Results == null)
            {
                return new List<Movie>();
            }

            foreach (var movie in movieListResult.Results)
            {
                if (movie.GenreIds != null)
                {
                    movie.Genres = movie.GenreIds
                        .Select(id => genresLookup.TryGetValue(id, out var genre) ? genre : null)
                        .Where(g => g != null)
                        .Cast<Movie.Genre>()
                        .ToList();
                }


                // Save the movie along with its genres to Neo4j
                await _neo4JService.SaveMovieAsync(movie);
            }

            return movieListResult.Results;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching popular movies");
            throw;
        }
    }

    public async Task<Dictionary<int, Movie.Genre>> GetGenresAsync()
    {
        var requestUri = $"{_baseUrl}genre/movie/list";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri)
        {
            Headers =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
                Authorization = new AuthenticationHeaderValue("Bearer", _apiReadAccessToken)
            }
        };

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var genresResult = JsonSerializer.Deserialize<GenresResult>(responseContent, JsonSerializerOptions);

        return genresResult?.Genres.ToDictionary(g => g.Id) ?? new Dictionary<int, Movie.Genre>();
    }
}