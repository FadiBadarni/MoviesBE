using Microsoft.AspNetCore.Mvc;
using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Services.TMDB;

namespace MoviesBE.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private readonly MovieDataService _movieDataService;

    public MoviesController(MovieDataService movieDataService)
    {
        _movieDataService = movieDataService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movie>> GetMovieById(int id)
    {
        var movie = await _movieDataService.GetMovieByIdAsync(id);
        return Ok(movie);
    }

    [HttpGet("tmdb/popular")]
    public async Task<ActionResult<List<Movie>>> GetTmdbPopularMovies()
    {
        var movies = await _movieDataService.GetTMDBPopularAndSave();
        return Ok(movies);
    }

    [HttpGet("tmdb/top-rated")]
    public async Task<ActionResult<List<Movie>>> GetTMDBTopRatedMovies()
    {
        var movies = await _movieDataService.GetTMDBTopRatedAndSave();
        return Ok(movies);
    }


    [HttpGet("popular")]
    public async Task<ActionResult<List<PopularMovie>>> GetPopularMovies([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (movies, totalMovies) = await _movieDataService.GetPopularMoviesAsync(page, pageSize);
        return Ok(new { movies, totalMovies });
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult<List<TopRatedMovie>>> GetTopRatedMovies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string filterType = "Default")
    {
        var (movies, totalMovies) = await _movieDataService.GetTopRatedMoviesAsync(page, pageSize, filterType);
        return Ok(new { movies, totalMovies });
    }


    [HttpGet("popular/limited")]
    public async Task<ActionResult<List<PopularMovie>>> GetLimitedPopularMovies()
    {
        var movies = await _movieDataService.GetLimitedPopularMoviesAsync();
        return Ok(movies);
    }

    [HttpGet("top-rated/limited")]
    public async Task<ActionResult<List<TopRatedMovie>>> GetLimitedTopRatedMovies()
    {
        var movies = await _movieDataService.GetLimitedTopRatedMoviesAsync();
        return Ok(movies);
    }

    [HttpGet("genres")]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        var genres = await _movieDataService.GetGenresAsync();
        return Ok(genres);
    }
}