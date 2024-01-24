using MoviesBE.Data;

namespace MoviesBE.Repositories;

public interface IMovieRepository
{
    Task SaveMovieAsync(Movie movie);
}