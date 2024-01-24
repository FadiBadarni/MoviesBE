using MoviesBE.Data;
using MoviesBE.Repositories;

namespace MoviesBE.Services.TMDB;

public class TmdbService
{
    private readonly IMovieRepository _movieRepository;
    private readonly TmdbApiService _tmdbApiService;

    public TmdbService(TmdbApiService tmdbApiService, IMovieRepository movieRepository)
    {
        _tmdbApiService = tmdbApiService;
        _movieRepository = movieRepository;
    }

    public async Task<Movie> GetMovieAsync(int movieId)
    {
        var movieInDb = await _movieRepository.GetMovieByIdAsync(movieId);
        if (movieInDb != null)
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
}