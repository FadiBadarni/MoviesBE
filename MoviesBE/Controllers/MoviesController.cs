using Microsoft.AspNetCore.Mvc;
using MoviesBE.Data;
using MoviesBE.Services;

namespace MoviesBE.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private readonly TmdbService _tmdbService;

    public MoviesController(TmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await _tmdbService.GetMovieAsync(id);
        return Ok(movie);
    }
}