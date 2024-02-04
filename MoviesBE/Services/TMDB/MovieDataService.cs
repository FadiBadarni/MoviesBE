using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Services.UserService;

namespace MoviesBE.Services.TMDB;

public class MovieDataService
{
    private const int HomePageMovieLimit = 3;
    private readonly AuthenticationService _authenticationService;
    private readonly IGenreRepository _genreRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly TmdbApiService _tmdbApiService;

    public MovieDataService(TmdbApiService tmdbApiService, IMovieRepository movieRepository,
        IGenreRepository genreRepository, AuthenticationService authenticationService)
    {
        _tmdbApiService = tmdbApiService;
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _authenticationService = authenticationService;
    }

    public async Task<Movie> GetMovieByIdAsync(int movieId)
    {
        var userId = _authenticationService.GetUserId();
        var movieInDb = await _movieRepository.GetMovieByIdAsync(movieId, userId);

        if (movieInDb != null && IsMovieDataComplete(movieInDb))
        {
            return movieInDb;
        }

        var movie = await _tmdbApiService.FetchMovieFromTmdbAsync(movieId);
        if (movie == null)
        {
            throw new KeyNotFoundException($"Movie with ID {movieId} not found.");
        }

        await _movieRepository.SaveMovieAsync(movie);
        return movie;
    }

    public async Task<(List<PopularMovie>, int)> GetPopularMoviesAsync(int page, int pageSize)
    {
        return await _movieRepository.GetPopularMoviesAsync(page, pageSize);
    }

    public async Task<(List<TopRatedMovie>, int)> GetTopRatedMoviesAsync(int page, int pageSize, string ratingFilter,
        int? genreFilter)
    {
        return await _movieRepository.GetTopRatedMoviesAsync(page, pageSize, ratingFilter, genreFilter);
    }

    public async Task<(List<TopRatedMovie>, int)> GetRecommendedMoviesAsync(int page, int pageSize, string ratingFilter,
        int? genreFilter)
    {
        return await _movieRepository.GetTopRatedMoviesAsync(page, pageSize, ratingFilter, genreFilter);
    }

    public async Task<List<Movie>> GetTMDBPopularAndSave()
    {
        var popularMovies = await _tmdbApiService.GetPopularMoviesAsync();

        foreach (var movie in popularMovies) await _movieRepository.SaveMovieAsync(movie);

        return popularMovies;
    }

    public async Task<List<Movie>> GetTMDBTopRatedAndSave()
    {
        var topRatedMovies = await _tmdbApiService.GetTopRatedMoviesAsync();

        foreach (var movie in topRatedMovies) await _movieRepository.SaveMovieAsync(movie);

        return topRatedMovies;
    }


    public bool IsMovieDataComplete(Movie movie)
    {
        var hasEssentialInfo = !string.IsNullOrEmpty(movie.Title) &&
                               !string.IsNullOrEmpty(movie.Overview) &&
                               !string.IsNullOrEmpty(movie.ReleaseDate) &&
                               movie.Genres != null && movie.Genres.Count > 0 &&
                               !string.IsNullOrEmpty(movie.PosterPath);

        var hasAdditionalInfo = movie.Runtime > 0 &&
                                !string.IsNullOrEmpty(movie.Status) &&
                                movie.VoteAverage > 0;

        var hasBackdropImages = movie.Backdrops != null && movie.Backdrops.Count > 0;
        var hasVideos = movie.Trailers != null && movie.Trailers.Count > 0;
        var hasCredits = movie.Credits != null &&
                         movie.Credits.Cast != null && movie.Credits.Cast.Count > 0 &&
                         movie.Credits.Crew != null && movie.Credits.Crew.Count > 0;

        return hasEssentialInfo && hasAdditionalInfo && hasBackdropImages && hasVideos && hasCredits;
    }

    public async Task<List<PopularMovie>> GetLimitedPopularMoviesAsync()
    {
        return await _movieRepository.GetLimitedPopularMoviesAsync(HomePageMovieLimit);
    }

    public async Task<List<TopRatedMovie>> GetLimitedTopRatedMoviesAsync()
    {
        return await _movieRepository.GetLimitedTopRatedMoviesAsync(HomePageMovieLimit);
    }

    public async Task<IEnumerable<Genre>> GetGenresAsync()
    {
        return await _genreRepository.GetGenresAsync();
    }
}