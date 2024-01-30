using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface ICreditsRepository
{
    Task SaveCreditsAsync(Credits credits, IAsyncQueryRunner tx);
    Task<List<CastMember>> GetMovieCastAsync(IAsyncQueryRunner tx, int movieId);
    Task<List<CrewMember>> GetMovieCrewAsync(IAsyncQueryRunner tx, int movieId);
}