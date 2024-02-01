using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Repositories.Implementations;
using MoviesBE.Services.User;

namespace MoviesBE.Services.UserService;

public class UserService
{
    private readonly Auth0Client _auth0Client;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, Auth0Client auth0Client,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _auth0Client = auth0Client;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Entities.User> RegisterOrUpdateUserAsync()
    {
        var accessToken = GetAccessTokenFromHttpContext();

        var userInfo = await _auth0Client.GetUserInfoAsync(accessToken);

        var existingUser = await _userRepository.FindByAuth0IdAsync(userInfo.Sub);
        if (existingUser != null)
        {
            UpdateExistingUser(existingUser, userInfo);
            await _userRepository.UpdateAsync(existingUser);
            return existingUser;
        }

        var newUser = MapUserInfoToUser(userInfo);
        await _userRepository.AddAsync(newUser);
        return newUser;
    }


    private void UpdateExistingUser(Entities.User existingUser, UserInfo userInfo)
    {
        // Update user properties
        existingUser.Email = userInfo.Email;
        existingUser.FullName = userInfo.Name;
        existingUser.ProfilePicture = userInfo.Picture;
        existingUser.EmailVerified = userInfo.EmailVerified;
    }

    private static Entities.User MapUserInfoToUser(UserInfo userInfo)
    {
        // Create a new User entity from UserInfo
        return new Entities.User
        {
            Auth0Id = userInfo.Sub,
            Email = userInfo.Email,
            FullName = userInfo.Name,
            ProfilePicture = userInfo.Picture,
            EmailVerified = userInfo.EmailVerified,
            Role = Role.User,
            Language = userInfo.Locale
        };
    }

    private string GetAccessTokenFromHttpContext()
    {
        const string bearerPrefix = "Bearer ";

        var httpContext = _httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("HttpContext is not available.");

        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues)
            || string.IsNullOrWhiteSpace(authorizationHeaderValues))
        {
            throw new InvalidOperationException("Authorization header is missing.");
        }

        var authorizationHeader = authorizationHeaderValues.ToString();
        if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid authorization header format.");
        }

        var token = authorizationHeader[bearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Access token is missing in the authorization header.");
        }

        return token;
    }
}