using MoviesBE.DTOs;
using MoviesBE.Entities;

namespace MoviesBE.Services;

public class TmdbApiService
{
    private readonly string _baseUrl;
    private readonly HttpService _httpService;

    public TmdbApiService(HttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
        _baseUrl = configuration["TMDB:BaseUrl"] ?? throw new InvalidOperationException("Base URL is not configured.");
    }

    public async Task<Movie?> FetchMovieFromTmdbAsync(int movieId)
    {
        var requestUri = $"{_baseUrl}movie/{movieId}";
        var movieResponse = await _httpService.SendAndDeserializeAsync<Movie>(requestUri);

        if (movieResponse == null)
        {
            throw new InvalidOperationException($"No movie data returned for movie ID {movieId}.");
        }

        var backdrops = await FetchMovieBackdropsAsync(movieId);
        movieResponse.Backdrops = backdrops.Take(5).ToList();

        return movieResponse;
    }

    private async Task<List<MovieBackdrop>> FetchMovieBackdropsAsync(int movieId)
    {
        var backdropsUri = $"{_baseUrl}movie/{movieId}/images";

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
}