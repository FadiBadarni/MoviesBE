using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Services.TMDB;

namespace MoviesBE.Services;

public class TmdbApiService
{
    private readonly string _baseUrl;
    private readonly HttpService _httpService;
    private readonly MovieBackdropService _movieBackdropService;

    public TmdbApiService(HttpService httpService, IConfiguration configuration,
        MovieBackdropService movieBackdropService)
    {
        _httpService = httpService;
        _baseUrl = configuration["TMDB:BaseUrl"] ?? throw new InvalidOperationException("Base URL is not configured.");
        _movieBackdropService = movieBackdropService;
    }

    public async Task<Movie?> FetchMovieFromTmdbAsync(int movieId)
    {
        var requestUri = $"{_baseUrl}movie/{movieId}";
        var movieResponse = await _httpService.SendAndDeserializeAsync<Movie>(requestUri);

        if (movieResponse == null)
        {
            throw new InvalidOperationException($"No movie data returned for movie ID {movieId}.");
        }

        var backdrops = await _movieBackdropService.FetchMovieBackdropsAsync(movieId);
        movieResponse.Backdrops = backdrops;

        return movieResponse;
    }


    public async Task<List<Movie>> GetPopularMoviesAsync()
    {
        var genresLookup = await GetGenresAsync();
        var movieListResult = await _httpService.SendAndDeserializeAsync<MovieListResult>($"{_baseUrl}movie/popular");

        if (movieListResult?.Results == null)
        {
            return new List<Movie>();
        }

        var moviesWithGenres = new List<Movie>();

        foreach (var movie in movieListResult.Results)
        {
            if (movie.GenreIds != null)
            {
                movie.Genres = movie.GenreIds
                    .Select(id => genresLookup.GetValueOrDefault(id))
                    .OfType<Genre>()
                    .ToList();
            }

            moviesWithGenres.Add(movie);
        }

        return moviesWithGenres;
    }

    private async Task<Dictionary<int, Genre>> GetGenresAsync()
    {
        var requestUri = $"{_baseUrl}genre/movie/list";
        var genresResult = await _httpService.SendAndDeserializeAsync<GenresResult>(requestUri);

        return genresResult?.Genres?.ToDictionary(g => g.Id) ?? new Dictionary<int, Genre>();
    }

    public async Task<List<MovieVideo>> FetchMovieVideosAsync(int movieId)
    {
        var requestUri = $"{_baseUrl}movie/{movieId}/videos";
        var videosResponse = await _httpService.SendAndDeserializeAsync<MovieVideosResponse>(requestUri);

        return videosResponse?.Videos ?? new List<MovieVideo>();
    }
}