using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class GenreRepository : IGenreRepository
{
    private readonly IDriver _neo4JDriver;

    public GenreRepository(IDriver neo4JDriver)
    {
        _neo4JDriver = neo4JDriver;
    }

    public async Task SaveGenresAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.Genres == null)
        {
            return;
        }

        // Remove existing genre relationships from this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:HAS_GENRE]->(g:Genre)
          DELETE r",
            new { movieId = movie.Id });

        // Merge each genre and create a new relationship with the movie.
        foreach (var genre in movie.Genres)
            await tx.RunAsync(
                @"MERGE (g:Genre {id: $id})
              ON CREATE SET g.name = $name
              ON MATCH SET g.name = $name
              WITH g
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:HAS_GENRE]->(g)",
                new { id = genre.Id, name = genre.Name, movieId = movie.Id });
    }

    public async Task<IEnumerable<Genre>> GetGenresAsync()
    {
        // Implementation to fetch genres from the database
        // Example with a Neo4j query:
        var query = "MATCH (g:Genre) RETURN g";
        var genres = new List<Genre>();

        // Assuming 'tx.RunAsync' executes a Neo4j query and returns results
        await using var session = _neo4JDriver.AsyncSession();
        var cursor = await session.RunAsync(query);
        while (await cursor.FetchAsync())
        {
            var record = cursor.Current;
            var genreNode = record["g"].As<INode>();
            genres.Add(new Genre
            {
                Id = genreNode.Properties["id"].As<int>(),
                Name = genreNode.Properties["name"].As<string>()
            });
        }

        return genres;
    }

    public async Task<List<Genre>> GetMovieGenresAsync(IAsyncQueryRunner tx, int movieId)
    {
        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie)-[:HAS_GENRE]->(g:Genre) WHERE m.id = $id RETURN COLLECT(DISTINCT g) as genres",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["genres"].As<List<INode>>()
                .Select(GenreNodeConverter.ConvertNodeToGenre).ToList();
        }

        return new List<Genre>();
    }
}