using MoviesBE.DTOs;
using MoviesBE.Repositories.Interfaces;
using Neo4j.Driver;

namespace MoviesBE.Services.UserService;

public class RecommendationService
{
    private readonly IGenreRepository _genreRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IDriver _neo4JDriver;
    private readonly IUserRepository _userRepository;

    public RecommendationService(IMovieRepository movieRepository, IUserRepository userRepository, IDriver neo4JDriver,
        IGenreRepository genreRepository)
    {
        _movieRepository = movieRepository;
        _userRepository = userRepository;
        _neo4JDriver = neo4JDriver;
        _genreRepository = genreRepository;
    }

    public async Task<(List<RecommendedMovie>, int)> GetRecommendedMoviesAsync(string userId, int page, int pageSize,
        string ratingFilter = null, int? genreFilter = null)
    {
        // Find similar users based on movie interactions
        var similarUsers = await FindSimilarUsersAsync(userId);

        // Calculate skip based on page and pageSize
        var skip = (page - 1) * pageSize;

        // Recommend movies from similar users with pagination
        var (recommendedMovies, totalRecommendations) =
            await RecommendMoviesFromSimilarUsersAsync(userId, similarUsers, skip, pageSize);

        // Return the paginated list of movies and the total count of recommendations
        return (recommendedMovies, totalRecommendations);
    }


    private async Task<List<string>> FindSimilarUsersAsync(string userId)
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

    private async Task<(List<RecommendedMovie>, int)> RecommendMoviesFromSimilarUsersAsync(string userId,
        List<string> similarUserIds, int skip, int pageSize)
    {
        var recommendedMovies = new List<RecommendedMovie>();
        var totalRecommendations = 0;

        await using var session = _neo4JDriver.AsyncSession();
        await session.ExecuteReadAsync(async tx =>
        {
            // First, get the total count of recommendations
            var countQuery = @"MATCH (movie:Movie)<-[rel:VIEWED|BOOKMARKED]-(user:User)
                           WHERE user.auth0Id IN $similarUserIds AND NOT (:User {auth0Id: $userId})-[:VIEWED|BOOKMARKED]->(movie)
                           RETURN COUNT(DISTINCT movie) AS totalCount";
            var countResult = await tx.RunAsync(countQuery, new { userId, similarUserIds });
            if (await countResult.FetchAsync())
            {
                totalRecommendations = countResult.Current["totalCount"].As<int>();
            }

            // Then, query for the paginated list of recommended movies
            var query = @"MATCH (movie:Movie)<-[rel:VIEWED|BOOKMARKED]-(user:User)
                      WHERE user.auth0Id IN $similarUserIds AND NOT (:User {auth0Id: $userId})-[:VIEWED|BOOKMARKED]->(movie)
                      WITH movie, SUM(rel.weight) AS totalWeight
                      RETURN movie, totalWeight ORDER BY totalWeight DESC SKIP $skip LIMIT $pageSize";
            var cursor = await tx.RunAsync(query, new { userId, similarUserIds, skip, pageSize });
            var records = await cursor.ToListAsync();

            var maxWeight = records.Max(record => record["totalWeight"].As<double>());

            foreach (var record in records)
            {
                var movieNode = record["movie"].As<INode>();
                var movieId = movieNode["id"].As<int>();
                var genres = await _genreRepository.GetMovieGenresAsync(tx, movieId);

                var totalWeight = record["totalWeight"].As<double>();
                var recommendationScore = totalWeight / maxWeight * 100;

                var movie = new RecommendedMovie
                {
                    Id = movieId,
                    Title = movieNode["title"].As<string>(),
                    PosterPath = movieNode["posterPath"].As<string>(),
                    ReleaseDate = movieNode["releaseDate"].As<string>(),
                    Overview = movieNode["overview"].As<string>(),
                    Runtime = movieNode["runtime"].As<int>(),
                    Genres = genres,
                    RecommendationScore = recommendationScore
                };
                recommendedMovies.Add(movie);
            }
        });

        return (recommendedMovies, totalRecommendations);
    }
}