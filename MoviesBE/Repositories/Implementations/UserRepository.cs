using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class UserRepository : IUserRepository
{
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
            return UserNodeConverter.ConvertNodeToUser(userNode);
        }

        return null;
    }
}