using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IMVideoRepository
{
    Task SaveMovieVideosAsync(Movie movie, IAsyncQueryRunner tx);
    Task<List<MovieVideo>> GetMovieVideosAsync(IAsyncQueryRunner tx, int movieId);
}