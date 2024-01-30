using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class PCountryRepository : IPCountryRepository
{
    public async Task SaveProductionCountriesAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.ProductionCountries == null)
        {
            return;
        }

        // First, detach all existing production country relationships from this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:PRODUCED_IN]->(c:Country)
          DELETE r",
            new { movieId = movie.Id });

        // Then, merge each production country and create a relationship with the movie.
        foreach (var country in movie.ProductionCountries)
            await tx.RunAsync(
                @"MERGE (c:Country {iso31661: $iso31661})
              ON CREATE SET c.name = $name
              ON MATCH SET c.name = $name
              WITH c
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:PRODUCED_IN]->(c)",
                new { iso31661 = country.Iso31661, name = country.Name, movieId = movie.Id });
    }

    public async Task<List<ProductionCountry>> GetMovieProductionCountriesAsync(IAsyncQueryRunner tx, int movieId)
    {
        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie)-[:PRODUCED_IN]->(pc:Country) WHERE m.id = $id RETURN COLLECT(DISTINCT pc) as countries",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["countries"].As<List<INode>>()
                .Select(CountryNodeConverter.ConvertNodeToCountry).ToList();
        }

        return new List<ProductionCountry>();
    }
}