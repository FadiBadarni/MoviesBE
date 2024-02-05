using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private const double BookmarkWeight = 2.0;
    private readonly ILogger<UserRepository> _logger;
    private readonly IDriver _neo4JDriver;

    public UserRepository(IDriver neo4JDriver, ILogger<UserRepository> logger)
    {
        _neo4JDriver = neo4JDriver;
        _logger = logger;
    }

    public async Task<User?> FindByAuth0IdAsync(string auth0Id)
    {
        try
        {
            await using var session = _neo4JDriver.AsyncSession();
            var user = await session.ExecuteReadAsync(async tx => await FindUserByAuth0Id(tx, auth0Id));
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by Auth0 ID.");
            throw;
        }
    }

    public async Task AddOrUpdateAsync(User user)
    {
        await using var session = _neo4JDriver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(tx => MergeUser(tx, user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding or updating user.");
            throw;
        }
    }

    public async Task<bool> BookmarkMovieAsync(string userId, int movieId)
    {
        await using var session = _neo4JDriver.AsyncSession();

        await session.ExecuteWriteAsync(async tx =>
        {
            // Check if a VIEWED relationship already exists and if so, get its weight
            var viewedResult = await tx.RunAsync(
                @"
            MATCH (u:User {auth0Id: $userId})-[r:VIEWED]->(m:Movie {id: $movieId})
            RETURN r.weight AS viewWeight",
                new { userId, movieId });

            double existingViewWeight = 0;
            if (await viewedResult.FetchAsync())
            {
                existingViewWeight = viewedResult.Current["viewWeight"].As<double>();
            }

            // Merge the BOOKMARKED relationship with a weight that's the sum of the view weight and bookmark weight
            var bookmarkResult = await tx.RunAsync(
                @"
            MATCH (u:User {auth0Id: $userId}), (m:Movie {id: $movieId})
            MERGE (u)-[r:BOOKMARKED]->(m)
            ON CREATE SET r.weight = $bookmarkWeight + $existingViewWeight
            ON MATCH SET r.weight = $bookmarkWeight + $existingViewWeight
            RETURN id(m) AS movieId",
                new { userId, movieId, bookmarkWeight = BookmarkWeight, existingViewWeight });

            return await bookmarkResult.FetchAsync();
        });
        return true;
    }


    public async Task<List<int>> FetchWatchlistAsync(string userId)
    {
        var watchlist = new List<int>();
        await using var session = _neo4JDriver.AsyncSession();
        await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"
            MATCH (u:User {auth0Id: $userId})-[:BOOKMARKED]->(m:Movie)
            RETURN m.id AS movieId",
                new { userId });

            while (await cursor.FetchAsync()) watchlist.Add(cursor.Current["movieId"].As<int>());
        });

        return watchlist;
    }

    public async Task<bool> UnbookmarkMovieAsync(string userId, int movieId)
    {
        await using var session = _neo4JDriver.AsyncSession();
        return await session.ExecuteWriteAsync(async tx =>
        {
            // Attempt to decrease the weight of the VIEWED relationship by the bookmark weight if it exists and is greater than the decrement
            await tx.RunAsync(
                @"MATCH (u:User {auth0Id: $userId})-[r:VIEWED]->(m:Movie {id: $movieId})
                        WHERE r.weight IS NOT NULL AND r.weight > $decrement
                        SET r.weight = CASE WHEN r.weight - $decrement < 0 THEN 0 ELSE r.weight - $decrement END",
                new { userId, movieId, decrement = BookmarkWeight });

            // Delete the BOOKMARKED relationship
            var result = await tx.RunAsync(
                @"MATCH (u:User {auth0Id: $userId})-[r:BOOKMARKED]->(m:Movie {id: $movieId})
                        DELETE r
                        RETURN COUNT(r) > 0 AS unbookmarked",
                new { userId, movieId });

            if (await result.FetchAsync())
            {
                return result.Current["unbookmarked"].As<bool>();
            }

            return false;
        });
    }

    private async Task MergeUser(IAsyncQueryRunner tx, User user)
    {
        var query = @"MERGE (u:User {auth0Id: $auth0Id})
                      ON CREATE SET
                        u.email = $email, 
                        u.fullName = $fullName, 
                        u.profilePicture = $profilePicture, 
                        u.emailVerified = $emailVerified, 
                        u.role = $role, 
                        u.language = $language
                      ON MATCH SET
                        u.email = $email, 
                        u.fullName = $fullName, 
                        u.profilePicture = $profilePicture, 
                        u.emailVerified = $emailVerified, 
                        u.role = $role, 
                        u.language = $language";
        await tx.RunAsync(query, GetUserParameters(user));
    }

    private static object GetUserParameters(User user)
    {
        return new
        {
            auth0Id = user.Auth0Id,
            email = user.Email,
            fullName = user.FullName,
            profilePicture = user.ProfilePicture,
            emailVerified = user.EmailVerified,
            role = user.Role.ToString(),
            language = user.Language
        };
    }

    private static async Task<User?> FindUserByAuth0Id(IAsyncQueryRunner tx, string auth0Id)
    {
        var cursor = await tx.RunAsync("MATCH (u:User {auth0Id: $auth0Id}) RETURN u", new { auth0Id });
        if (await cursor.FetchAsync())
        {
            var userNode = cursor.Current["u"].As<INode>();
            return UserConverter.ConvertNodeToUser(userNode);
        }

        return null;
    }
}