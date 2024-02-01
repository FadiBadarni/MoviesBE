using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Exceptions;
using MoviesBE.Repositories.Interfaces;

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

    public async Task<User> RegisterOrUpdateUserAsync()
    {
        var accessToken = GetAccessTokenFromHttpContext();

        UserInfo userInfo;
        try
        {
            userInfo = await _auth0Client.GetUserInfoAsync(accessToken);
        }
        catch (Exception ex)
        {
            throw new UserRegistrationException("Error retrieving user information from Auth0.", ex);
        }

        if (userInfo == null)
        {
            throw new UserRegistrationException("User information is null after retrieval from Auth0.");
        }

        if (string.IsNullOrEmpty(userInfo.Sub))
        {
            throw new UserRegistrationException("Auth0 ID is missing in the user information.");
        }

        var user = MapUserInfoToUser(userInfo);

        await _userRepository.AddOrUpdateAsync(user);

        return user;
    }

    private static User MapUserInfoToUser(UserInfo userInfo)
    {
        return new User
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