using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
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

    public async Task UpdateAsync(User user)
    {
        await using var session = _neo4JDriver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync("MATCH (u:User {auth0Id: $auth0Id}) " +
                              "SET u.email = $email, u.fullName = $fullName, " +
                              "u.profilePicture = $profilePicture, u.emailVerified = $emailVerified, " +
                              "u.role = $role, u.language = $language",
                new
                {
                    auth0Id = user.Auth0Id,
                    email = user.Email,
                    fullName = user.FullName,
                    profilePicture = user.ProfilePicture,
                    emailVerified = user.EmailVerified,
                    role = user.Role.ToString(),
                    language = user.Language
                });
        });
    }

    public async Task AddAsync(User user)
    {
        await using var session = _neo4JDriver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync("CREATE (u:User {auth0Id: $auth0Id, email: $email, fullName: $fullName, " +
                              "profilePicture: $profilePicture, emailVerified: $emailVerified, role: $role, " +
                              "language: $language})",
                new
                {
                    auth0Id = user.Auth0Id,
                    email = user.Email,
                    fullName = user.FullName,
                    profilePicture = user.ProfilePicture,
                    emailVerified = user.EmailVerified,
                    role = user.Role.ToString(),
                    language = user.Language
                });
        });
    }

    private async Task<User?> FindUserByAuth0Id(IAsyncQueryRunner tx, string auth0Id)
    {
        var cursor = await tx.RunAsync("MATCH (u:User {auth0Id: $auth0Id}) RETURN u", new { auth0Id });
        if (await cursor.FetchAsync())
        {
            var userNode = cursor.Current["u"].As<INode>();
            return ConvertNodeToUser(userNode);
        }

        return null;
    }

    private User ConvertNodeToUser(INode node)
    {
        return new User
        {
            Auth0Id = node.Properties["auth0Id"].As<string>(),
            Email = node.Properties["email"].As<string>(),
            FullName = node.Properties["fullName"].As<string>(),
            ProfilePicture = node.Properties["profilePicture"].As<string>(),
            EmailVerified = node.Properties["emailVerified"].As<bool>(),
            Role = Enum.Parse<Role>(node.Properties["role"].As<string>()),
            Language = node.Properties["language"].As<string>()
        };
    }
}