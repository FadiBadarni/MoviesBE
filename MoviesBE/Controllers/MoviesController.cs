using Microsoft.AspNetCore.Mvc;
using MoviesBE.Data;
using MoviesBE.Repositories;
using MoviesBE.Services.Database;
using MoviesBE.Services.TMDB;

namespace MoviesBE.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;
    private readonly Neo4JService _neo4JService;
    private readonly TmdbService _tmdbService;

    public MoviesController(TmdbService tmdbService, Neo4JService neo4JService, IMovieRepository movieRepository)
    {
        _tmdbService = tmdbService;
        _neo4JService = neo4JService;
        _movieRepository = movieRepository;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await _tmdbService.GetMovieAsync(id);
        return Ok(movie);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<List<Movie>>> GetPopularMovies()
    {
        var movies = await _tmdbService.GetPopularMoviesAndSaveAsync();
        return Ok(movies);
    }

    [HttpGet("cached-popular")]
    public async Task<ActionResult<List<PopularMovie>>> GetCachedPopularMovies()
    {
        var movies = await _movieRepository.GetCachedPopularMoviesAsync();
        return Ok(movies);
    }
}