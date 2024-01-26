using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDriver _neo4JDriver;

    public RatingRepository(IDriver neo4JDriver)
    {
        _neo4JDriver = neo4JDriver;
    }

    public async Task UpdateMovieRatingsAsync(int movieId, List<Rating> ratings)
    {
        await using var session = _neo4JDriver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => { await SaveMovieRatingsAsync(movieId, ratings, tx); });
    }

    public async Task SaveMovieRatingsAsync(int movieId, List<Rating> ratings, IAsyncQueryRunner tx)
    {
        // First, remove existing ratings for this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:HAS_RATING]->(rating:Rating)
              DELETE r",
            new { movieId });

        // Then, save each new rating and create a relationship with the movie.
        foreach (var rating in ratings)
            await tx.RunAsync(
                @"MERGE (r:Rating {source: $source})
                  ON CREATE SET r.score = $score
                  ON MATCH SET r.score = $score
                  WITH r
                  MATCH (m:Movie {id: $movieId})
                  MERGE (m)-[:HAS_RATING]->(r)",
                new { source = rating.Provider, score = rating.Score, movieId });
    }
}