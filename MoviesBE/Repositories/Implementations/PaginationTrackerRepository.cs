using MoviesBE.Repositories.Interfaces;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class PaginationTrackerRepository : IPaginationTrackerRepository
{
    private readonly IDriver _neo4JDriver;

    public PaginationTrackerRepository(IDriver neo4JDriver)
    {
        _neo4JDriver = neo4JDriver;
    }

    public async Task UpdateLastFetchedPageAsync(string category, int lastFetchedPage)
    {
        await using var session = _neo4JDriver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(
                @"MERGE (p:PaginationTracker {category: $category})
                  ON CREATE SET p.lastFetchedPage = $lastFetchedPage
                  ON MATCH SET p.lastFetchedPage = $lastFetchedPage",
                new { category, lastFetchedPage });
        });
    }

    public async Task<int> GetLastFetchedPageAsync(string category)
    {
        await using var session = _neo4JDriver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (p:PaginationTracker {category: $category})
              RETURN p.lastFetchedPage AS lastFetchedPage",
                new { category });

            if (await cursor.FetchAsync())
            {
                return cursor.Current["lastFetchedPage"].As<int>();
            }

            return 0; // Default to 0 if not found
        });
    }
}