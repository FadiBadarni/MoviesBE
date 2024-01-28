using MoviesBE.DTOs;
using MoviesBE.Entities;

namespace MoviesBE.Repositories.Interfaces;

public interface IMovieRepository
{
    Task SaveMovieAsync(Movie movie);

    Task<Movie?> GetMovieByIdAsync(int movieId);

    Task<List<PopularMovie>> GetPopularMoviesAsync();

    Task<List<TopRatedMovie>> GetTopRatedMoviesAsync();

    Task<List<Movie>> GetMoviesWithoutIMDbRatingAsync();

    Task<List<Movie>> GetMoviesWithoutRTRatingAsync();
    Task<List<Movie>> GetAllMoviesAsync();
    Task<List<PopularMovie>> GetLimitedPopularMoviesAsync(int limit);
    Task<List<TopRatedMovie>> GetLimitedTopRatedMoviesAsync(int limit);
}