using MoviesBE.DTOs;
using MoviesBE.Entities;

namespace MoviesBE.Services.TMDB;

public class TmdbApiService
{
    private readonly string _baseUrl;
    private readonly CreditsService _creditsService;
    private readonly HttpService _httpService;
    private readonly MovieBackdropService _movieBackdropService;
    private readonly VideoService _videoService;

    public TmdbApiService(HttpService httpService, IConfiguration configuration,
        MovieBackdropService movieBackdropService, VideoService videoService, CreditsService creditsService)
    {
        _httpService = httpService;
        _baseUrl = configuration["TMDB:BaseUrl"] ?? throw new InvalidOperationException("Base URL is not configured.");
        _movieBackdropService = movieBackdropService;
        _videoService = videoService;
        _creditsService = creditsService;
    }

    public async Task<Movie?> FetchMovieFromTmdbAsync(int movieId)
    {
        var requestUri = $"{_baseUrl}movie/{movieId}";
        var movieResponse = await _httpService.SendAndDeserializeAsync<Movie>(requestUri);

        if (movieResponse == null)
        {
            throw new InvalidOperationException($"No movie data returned for movie ID {movieId}.");
        }

        // Fetch and set backdrops
        var backdrops = await _movieBackdropService.FetchMovieBackdropsAsync(movieId);
        movieResponse.Backdrops = backdrops;

        // Fetch and set videos
        var videos = await FetchMovieVideosAsync(movieId);
        movieResponse.Trailers = _videoService.OrganizeMovieVideos(videos);

        // Fetch and set credits
        var credits = await FetchMovieCreditsAsync(movieId);
        movieResponse.Credits = credits;

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

    public async Task<Credits> FetchMovieCreditsAsync(int movieId)
    {
        var requestUri = $"{_baseUrl}movie/{movieId}/credits";
        var creditsResponse = await _httpService.SendAndDeserializeAsync<Credits>(requestUri);

        if (creditsResponse == null)
        {
            throw new InvalidOperationException($"No credits data returned for movie ID {movieId}.");
        }

        return _creditsService.ProcessCredits(creditsResponse);
    }
}