using MoviesBE.Data;

namespace MoviesBE.Repositories;

public interface IMovieRepository
{
    Task SaveMovieAsync(Movie movie);

    Task<Movie?> GetMovieByIdAsync(int movieId);
}