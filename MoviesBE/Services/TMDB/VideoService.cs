using MoviesBE.Entities;
using MoviesBE.Repositories;

namespace MoviesBE.Services.TMDB;

public class VideoService
{
    private readonly IMovieRepository _movieRepository;
    private readonly TmdbApiService _tmdbApiService;

    public VideoService(TmdbApiService tmdbApiService, IMovieRepository movieRepository)
    {
        _tmdbApiService = tmdbApiService;
        _movieRepository = movieRepository;
    }

    public async Task<List<MovieVideo>> FetchMovieVideosAsync(int movieId)
    {
        return await _tmdbApiService.FetchMovieVideosAsync(movieId);
    }
}