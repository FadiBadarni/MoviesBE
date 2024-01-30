using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IMovieCollectionRepository
{
    Task SaveMovieCollectionAsync(Movie movie, IAsyncQueryRunner tx);
}