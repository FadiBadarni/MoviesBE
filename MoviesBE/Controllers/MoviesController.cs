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
    private readonly MovieDataService _movieDataService;
    private readonly MovieVideoOrganizerService _movieVideoOrganizerService;

    public MoviesController(MovieDataService movieDataService, IMovieRepository movieRepository, MovieVideoOrganizerService movieVideoOrganizerService)
    {
        _movieDataService = movieDataService;
        _movieRepository = movieRepository;
        _movieVideoOrganizerService = movieVideoOrganizerService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await _movieDataService.GetMovieAsync(id);
        return Ok(movie);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<List<Movie>>> GetPopularMovies()
    {
        var movies = await _movieDataService.GetPopularMoviesAndSaveAsync();
        return Ok(movies);
    }

    [HttpGet("cached-popular")]
    public async Task<ActionResult<List<PopularMovie>>> GetCachedPopularMovies()
    {
        var movies = await _movieRepository.GetCachedPopularMoviesAsync();
        return Ok(movies);
    }


    [HttpGet("top-rated")]
    public async Task<ActionResult<List<Movie>>> GetTopRatedMovies()
    {
        var movies = await _movieDataService.GetTopRatedMoviesAndSaveAsync();
        return Ok(movies);
    }

    [HttpGet("cached-top-rated")]
    public async Task<ActionResult<List<TopRatedMovie>>> GetCachedTopRatedMovies()
    {
        var movies = await _movieRepository.GetCachedTopRatedMoviesAsync();
        return Ok(movies);
    }
}