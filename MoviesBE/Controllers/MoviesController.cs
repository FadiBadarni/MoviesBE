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

    [HttpGet("popular")]
    public async Task<ActionResult<List<PopularMovie>>> GetPopularMovies()
    {
        var movies = await _movieDataService.GetPopularMoviesAsync();
        return Ok(movies);
    }


    [HttpGet("tmdb/top-rated")]
    public async Task<ActionResult<List<Movie>>> GetTMDBTopRatedMovies()
    {
        var movies = await _movieDataService.GetTMDBTopRatedAndSave();
        return Ok(movies);
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult<List<TopRatedMovie>>> GetTopRatedMovies()
    {
        var movies = await _movieDataService.GetTopRatedMoviesAsync();
        return Ok(movies);
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
}