using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories;

public interface ICreditsRepository
{
    Task SaveCreditsAsync(Credits credits, int movieId, IAsyncQueryRunner tx);
}