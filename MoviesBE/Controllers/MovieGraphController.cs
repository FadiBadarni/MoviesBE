using Microsoft.AspNetCore.Mvc;
using MoviesBE.Entities;
using MoviesBE.Services.Graph;

namespace MoviesBE.Controllers;

[ApiController]
[Route("movies/")]
public class MovieGraphController : ControllerBase
{
    private readonly GraphDataService _graphDataService;

    public MovieGraphController(GraphDataService graphDataService)
    {
        _graphDataService = graphDataService;
    }

    [HttpGet("graph/genres/{genreId:int}")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByGenre(int genreId)
    {
        var movies = await _graphDataService.GetMoviesByGenreAsync(genreId);
        return Ok(movies);
    }
}