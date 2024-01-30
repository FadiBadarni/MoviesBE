using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class PCompanyRepository : IPCompanyRepository
{
    public async Task SaveProductionCompaniesAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.ProductionCompanies == null)
        {
            return;
        }

        // First, detach all existing production company relationships from this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:PRODUCED_BY]->(c:Company)
          DELETE r",
            new { movieId = movie.Id });

        // Then, merge each production company and create a relationship with the movie.
        foreach (var company in movie.ProductionCompanies)
            await tx.RunAsync(
                @"MERGE (c:Company {id: $id})
              ON CREATE SET c.name = $name, c.logoPath = $logoPath, c.originCountry = $originCountry
              ON MATCH SET c.name = $name, c.logoPath = $logoPath, c.originCountry = $originCountry
              WITH c
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:PRODUCED_BY]->(c)",
                new
                {
                    id = company.Id, name = company.Name, logoPath = company.LogoPath,
                    originCountry = company.OriginCountry, movieId = movie.Id
                });
    }

    public async Task<List<ProductionCompany>> GetMovieProductionCompaniesAsync(IAsyncQueryRunner tx, int movieId)
    {
        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie)-[:PRODUCED_BY]->(c:Company) WHERE m.id = $id RETURN COLLECT(DISTINCT c) as companies",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["companies"].As<List<INode>>()
                .Select(CompanyNodeConverter.ConvertNodeToCompany).ToList();
        }

        return new List<ProductionCompany>();
    }
}