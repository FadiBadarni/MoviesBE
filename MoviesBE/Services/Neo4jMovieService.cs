namespace MoviesBE.Services;

public class Neo4jMovieService
{
    private readonly ILogger<Neo4jMovieService> _logger;
    private readonly Neo4JService _neo4JService;

    public Neo4jMovieService(Neo4JService neo4JService, ILogger<Neo4jMovieService> logger)
    {
        _neo4JService = neo4JService ?? throw new ArgumentNullException(nameof(neo4JService));
        _logger = logger;
    }
}