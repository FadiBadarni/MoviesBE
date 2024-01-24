using MoviesBE.Data;
using MoviesBE.DTOs;

namespace MoviesBE.Services;

public class TmdbService
{
    private readonly string _baseUrl;
    private readonly HttpService _httpService;
    private readonly ILogger<TmdbService> _logger;
    private readonly Neo4JService _neo4JService;

    public TmdbService(HttpService httpService, IConfiguration configuration, Neo4JService neo4JService,
        ILogger<TmdbService> logger)
    {
        _httpService = httpService;
        _baseUrl = configuration["TMDB:BaseUrl"] ?? throw new InvalidOperationException("Base URL is not configured.");
        _logger = logger;
        _neo4JService = neo4JService ?? throw new ArgumentNullException(nameof(neo4JService));
    }

    public async Task<ServiceResult<Movie>> GetMovieAsync(int movieId)
    {
        var movieInDb = await _neo4JService.GetMovieByIdAsync(movieId);
        if (movieInDb != null)
        {
            return new ServiceResult<Movie>(movieInDb, true, null);
        }

        try
        {
            var movie = await FetchMovieFromTmdbAsync(movieId);
            if (movie == null)
            {
                return new ServiceResult<Movie>(null, false, "Movie not found");
            }

            await _neo4JService.SaveMovieAsync(movie);
            return new ServiceResult<Movie>(movie, true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching movie data for movie ID {MovieId}", movieId);
            return new ServiceResult<Movie>(null, false, ex.Message);
        }
    }

    private async Task<Movie?> FetchMovieFromTmdbAsync(int movieId)
    {
        var requestUri = $"{_baseUrl}movie/{movieId}";
        var movieResponse = await _httpService.SendAndDeserializeAsync<Movie>(requestUri);
        var backdrops = await FetchMovieBackdropsAsync(movieId);
        movieResponse.Backdrops = backdrops.Take(5).ToList();
        return movieResponse;
    }

    private async Task<List<MovieBackdrop>> FetchMovieBackdropsAsync(int movieId)
    {
        var backdropsUri = $"{_baseUrl}movie/{movieId}/images";

        try
        {
            var images = await _httpService.SendAndDeserializeAsync<MovieImagesResponse>(backdropsUri);
            if (images?.Backdrops == null)
            {
                return new List<MovieBackdrop>();
            }

            return images.Backdrops.Select(backdrop => new MovieBackdrop
            {
                FilePath = backdrop.FilePath,
                VoteAverage = backdrop.VoteAverage
            }).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching backdrop data for movie ID {MovieId}", movieId);
            return new List<MovieBackdrop>();
        }
    }


    public async Task<List<Movie>> GetPopularMoviesAsync()
    {
        var genresLookup = await GetGenresAsync();
        var movieListResult = await _httpService.SendAndDeserializeAsync<MovieListResult>($"{_baseUrl}movie/popular");

        if (movieListResult?.Results == null)
        {
            return new List<Movie>();
        }

        foreach (var movie in movieListResult.Results)
        {
            if (movie.GenreIds != null)
            {
                movie.Genres = movie.GenreIds
                    .Select(id => genresLookup.GetValueOrDefault(id))
                    .OfType<Movie.Genre>()
                    .ToList();
            }


            await _neo4JService.SaveMovieAsync(movie);
        }

        return movieListResult.Results;
    }

    public async Task<Dictionary<int, Movie.Genre>> GetGenresAsync()
    {
        var requestUri = $"{_baseUrl}genre/movie/list";
        var genresResult = await _httpService.SendAndDeserializeAsync<GenresResult>(requestUri);

        return genresResult?.Genres.ToDictionary(g => g.Id) ?? new Dictionary<int, Movie.Genre>();
    }
}