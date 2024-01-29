using MoviesBE.DTOs;
using MoviesBE.Entities;

namespace MoviesBE.Repositories.Interfaces;

public interface IMovieRepository
{
    Task SaveMovieAsync(Movie movie);

    Task<Movie?> GetMovieByIdAsync(int movieId);

    Task<(List<PopularMovie>, int)> GetPopularMoviesAsync(int page, int pageSize);

    Task<(List<TopRatedMovie>, int)> GetTopRatedMoviesAsync(int page, int pageSize, string filterType);

    Task<List<Movie>> GetMoviesWithoutIMDbRatingAsync();

    Task<List<Movie>> GetMoviesWithoutRTRatingAsync();
    Task<List<Movie>> GetAllMoviesAsync();
    Task<List<PopularMovie>> GetLimitedPopularMoviesAsync(int limit);
    Task<List<TopRatedMovie>> GetLimitedTopRatedMoviesAsync(int limit);
}