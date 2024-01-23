using Microsoft.AspNetCore.Mvc;
using MoviesBE.Data;
using MoviesBE.Services;

namespace MoviesBE.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private readonly Neo4JService _neo4JService;
    private readonly TmdbService _tmdbService;

    public MoviesController(TmdbService tmdbService, Neo4JService neo4JService)
    {
        _tmdbService = tmdbService;
        _neo4JService = neo4JService;
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
        var movies = await _tmdbService.GetPopularMoviesAsync();
        return Ok(movies);
    }

    [HttpGet("cached-popular")]
    public async Task<ActionResult<List<PopularMovie>>> GetCachedPopularMovies()
    {
        var movies = await _neo4JService.GetCachedPopularMoviesAsync();
        return Ok(movies);
    }
}