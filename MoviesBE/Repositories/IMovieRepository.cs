using MoviesBE.DTOs;
using MoviesBE.Entities;

namespace MoviesBE.Repositories;

public interface IMovieRepository
{
    Task SaveMovieAsync(Movie movie);

    Task<Movie?> GetMovieByIdAsync(int movieId);

    Task<List<PopularMovie>> GetCachedPopularMoviesAsync();

    Task<List<TopRatedMovie>> GetCachedTopRatedMoviesAsync();
}