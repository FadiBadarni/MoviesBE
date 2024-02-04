using MoviesBE.DTOs;
using MoviesBE.Repositories.Interfaces;
using Neo4j.Driver;

namespace MoviesBE.Services.UserService;

public class RecommendationService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IDriver _neo4JDriver;
    private readonly IUserRepository _userRepository;

    public RecommendationService(IMovieRepository movieRepository, IUserRepository userRepository, IDriver neo4JDriver)
    {
        _movieRepository = movieRepository;
        _userRepository = userRepository;
        _neo4JDriver = neo4JDriver;
    }

    public async Task<(List<RecommendedMovie>, int)> GetRecommendedMoviesAsync(string userId, int page, int pageSize,
        string ratingFilter = null, int? genreFilter = null)
    {
        // Find similar users based on movie interactions
        var similarUsers = await FindSimilarUsersAsync(userId);
        
        // Recommend movies from similar users
        var recommendedMovies = await RecommendMoviesFromSimilarUsersAsync(userId, similarUsers);


        return (new List<RecommendedMovie>(), 0);
    }

    public async Task<List<string>> FindSimilarUsersAsync(string userId)
    {
        var similarUsers = new List<string>();

        await using var session = _neo4JDriver.AsyncSession();
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
            MATCH (currentUser:User {auth0Id: $userId})
            MATCH (currentUser)-[:VIEWED|BOOKMARKED]->(m:Movie)<-[:VIEWED|BOOKMARKED]-(similarUser:User)
            WHERE currentUser <> similarUser
            WITH similarUser, COUNT(DISTINCT m) AS sharedMovies
            ORDER BY sharedMovies DESC
            RETURN similarUser.auth0Id AS similarUserId, sharedMovies
            LIMIT 10";

            var cursor = await tx.RunAsync(query, new { userId });
            var records = await cursor.ToListAsync();
            return records;
        });

        foreach (var record in result) similarUsers.Add(record["similarUserId"].ToString());

        return similarUsers;
    }

    public async Task<List<RecommendedMovie>> RecommendMoviesFromSimilarUsersAsync(string userId,
        List<string> similarUserIds)
    {
        var recommendedMovies = new List<RecommendedMovie>();

        await using var session = _neo4JDriver.AsyncSession();
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var query = @"MATCH (movie:Movie)<-[:VIEWED|BOOKMARKED]-(user:User)
                        WHERE user.auth0Id IN $similarUserIds
                        AND NOT (:User {auth0Id: $userId})-[:VIEWED|BOOKMARKED]->(movie)
                        RETURN movie, COUNT(*) AS recommendations
                        ORDER BY recommendations DESC
                        LIMIT 10";

            var cursor = await tx.RunAsync(query, new { userId, similarUserIds });
            var records = await cursor.ToListAsync();
            return records;
        });

        foreach (var record in result)
        {
            var movie = new RecommendedMovie
            {
                // Map the record to your RecommendedMovie model
                // Example: MovieId = record["movie"]["id"].As<int>(),
                // Title = record["movie"]["title"].As<string>(),
                // Recommendations = record["recommendations"].As<int>()
            };
            recommendedMovies.Add(movie);
        }

        return recommendedMovies;
    }
}