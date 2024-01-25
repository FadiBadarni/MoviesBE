using Microsoft.AspNetCore.Mvc;
using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Repositories;
using MoviesBE.Services.TMDB;

namespace MoviesBE.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;
    private readonly TmdbService _tmdbService;
    private readonly VideoService _videoService;

    public MoviesController(TmdbService tmdbService, IMovieRepository movieRepository, VideoService videoService)
    {
        _tmdbService = tmdbService;
        _movieRepository = movieRepository;
        _videoService = videoService;
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