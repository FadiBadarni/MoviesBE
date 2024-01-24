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
}