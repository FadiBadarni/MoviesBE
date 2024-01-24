using MoviesBE.Data;
using MoviesBE.Repositories;

namespace MoviesBE.Services;

public class TmdbService
{
    private readonly IMovieRepository _movieRepository;
    private readonly Neo4JService _neo4JService;
    private readonly TmdbApiService _tmdbApiService;

    public TmdbService(TmdbApiService tmdbApiService, Neo4JService neo4JService, IMovieRepository movieRepository)
    {
        _tmdbApiService = tmdbApiService;
        _neo4JService = neo4JService ?? throw new ArgumentNullException(nameof(neo4JService));
        _movieRepository = movieRepository;
    }

    public async Task<Movie> GetMovieAsync(int movieId)
    {
        var movieInDb = await _neo4JService.GetMovieByIdAsync(movieId);
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

        foreach (var movie in popularMovies) await _neo4JService.SaveMovieAsync(movie);

        return popularMovies;
    }
}