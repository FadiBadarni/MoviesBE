using MoviesBE.Data;

namespace MoviesBE.Services;

public class TmdbService
{
    private readonly ILogger<TmdbService> _logger;
    private readonly Neo4JService _neo4JService;
    private readonly TmdbApiService _tmdbApiService;

    public TmdbService(TmdbApiService tmdbApiService, Neo4JService neo4JService, ILogger<TmdbService> logger)
    {
        _tmdbApiService = tmdbApiService;
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
            var movie = await _tmdbApiService.FetchMovieFromTmdbAsync(movieId);
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

    public async Task<ServiceResult<List<Movie>>> GetPopularMoviesAndSaveAsync()
    {
        try
        {
            var popularMovies = await _tmdbApiService.GetPopularMoviesAsync();

            foreach (var movie in popularMovies) await _neo4JService.SaveMovieAsync(movie);

            return new ServiceResult<List<Movie>>(popularMovies, true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching popular movies");
            return new ServiceResult<List<Movie>>(null, false, ex.Message);
        }
    }
}