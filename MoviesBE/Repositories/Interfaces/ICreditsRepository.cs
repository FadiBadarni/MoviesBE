using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface ICreditsRepository
{
    Task SaveCreditsAsync(Credits credits, IAsyncQueryRunner tx);
}