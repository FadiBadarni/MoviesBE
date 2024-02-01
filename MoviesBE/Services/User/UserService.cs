using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Repositories.Implementations;

namespace MoviesBE.Services.User;

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

    private Entities.User MapUserInfoToUser(UserInfo userInfo)
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
        var httpContext = _httpContextAccessor.HttpContext;
        var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            return authorizationHeader.Substring("Bearer ".Length).Trim();
        }

        throw new InvalidOperationException("No access token found in the HTTP context.");
    }
}