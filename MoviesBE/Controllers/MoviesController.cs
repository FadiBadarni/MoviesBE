using Microsoft.AspNetCore.Mvc;
using MoviesBE.Services;

namespace MoviesBE.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly TmdbService _tmdbService;

        public MoviesController(TmdbService tmdbService)
        {
            _tmdbService = tmdbService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovie(int id)
        {
            try
            {
                var movie = await _tmdbService.GetMovieAsync(id);
                return Ok(movie);
            }
            catch (Exception ex)
            {
                // Proper exception handling
                return StatusCode(500, ex.Message);
            }
        }
    }
}