using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class MovieCollectionRepository : IMovieCollectionRepository
{
    public async Task SaveMovieCollectionAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.BelongsToCollection == null)
        {
            return;
        }

        var collection = movie.BelongsToCollection;
        await tx.RunAsync(
            @"MERGE (c:MovieCollection {id: $collectionId})
              ON CREATE SET c.name = $name, c.posterPath = $posterPath, c.backdropPath = $backdropPath
              ON MATCH SET c.name = $name, c.posterPath = $posterPath, c.backdropPath = $backdropPath
              WITH c
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:PART_OF_COLLECTION]->(c)",
            new
            {
                collectionId = collection.Id,
                name = collection.Name,
                posterPath = collection.PosterPath,
                backdropPath = collection.BackdropPath,
                movieId = movie.Id
            });
    }
}