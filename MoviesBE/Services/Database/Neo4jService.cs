using MoviesBE.Data;
using Neo4j.Driver;

namespace MoviesBE.Services.Database;

public class Neo4JService : IAsyncDisposable
{
    private readonly IDriver _neo4JDriver;

    public Neo4JService(IDriver neo4JDriver)
    {
        _neo4JDriver = neo4JDriver;
    }

    public async ValueTask DisposeAsync()
    {
        await _neo4JDriver.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async Task<List<PopularMovie>> GetCachedPopularMoviesAsync()
    {
        var movies = new List<PopularMovie>();
        await using var session = _neo4JDriver.AsyncSession();
        const double popularityThreshold = 50;
        try
        {
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (m:Movie)
                          WHERE m.popularity >= $popularityThreshold
                          RETURN m",
                    new { popularityThreshold });

                var records = await cursor.ToListAsync();
                return records;
            });

            foreach (var record in result)
            {
                var movieNode = record["m"].As<INode>();
                var movie = ConvertNodeToPopularMovie(movieNode);
                movies.Add(movie);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            // Log or rethrow depending on your strategy
        }

        return movies;
    }

    private static PopularMovie ConvertNodeToPopularMovie(IEntity node)
    {
        return new PopularMovie
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Title = node.Properties.ContainsKey("title") ? node.Properties["title"].As<string>() : string.Empty,
            PosterPath = node.Properties.ContainsKey("posterPath")
                ? node.Properties["posterPath"].As<string>()
                : string.Empty,
            ReleaseDate = node.Properties.ContainsKey("releaseDate")
                ? node.Properties["releaseDate"].As<string>()
                : string.Empty,
            Overview = node.Properties.ContainsKey("overview") ? node.Properties["overview"].As<string>() : string.Empty
        };
    }
}