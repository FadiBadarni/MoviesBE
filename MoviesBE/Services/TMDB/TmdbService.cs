using MoviesBE.Entities;
using MoviesBE.Repositories;
using MoviesBE.Services.IMDB;

namespace MoviesBE.Services.TMDB;

public class TmdbService
{
    private readonly IMDbScrapingService _imDbScrapingService;
    private readonly IMovieRepository _movieRepository;
    private readonly TmdbApiService _tmdbApiService;

    public TmdbService(TmdbApiService tmdbApiService, IMovieRepository movieRepository,
        IMDbScrapingService imDbScrapingService)
    {
        _tmdbApiService = tmdbApiService;
        _movieRepository = movieRepository;
        _imDbScrapingService = imDbScrapingService;
    }

    public async Task<Movie> GetMovieAsync(int movieId)
    {
        var movieInDb = await _movieRepository.GetMovieByIdAsync(movieId);

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


    public async Task<List<Movie>> GetPopularMoviesAndSaveAsync()
    {
        var popularMovies = await _tmdbApiService.GetPopularMoviesAsync();

        foreach (var movie in popularMovies) await _movieRepository.SaveMovieAsync(movie);

        return popularMovies;
    }

    public async Task<List<Movie>> GetTopRatedMoviesAndSaveAsync()
    {
        var topRatedMovies = await _tmdbApiService.GetTopRatedMoviesAsync();

        foreach (var movie in topRatedMovies) await _movieRepository.SaveMovieAsync(movie);

        return topRatedMovies;
    }


    private static bool IsMovieDataComplete(Movie movie)
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
}