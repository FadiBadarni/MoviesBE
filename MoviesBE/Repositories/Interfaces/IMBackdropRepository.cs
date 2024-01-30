using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IMBackdropRepository
{
    Task SaveMovieBackdropsAsync(Movie movie, IAsyncQueryRunner tx);
    Task<List<MovieBackdrop>> GetMovieBackdropsAsync(IAsyncQueryRunner tx, int movieId);
}