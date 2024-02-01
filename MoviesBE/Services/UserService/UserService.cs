using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Exceptions;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;

namespace MoviesBE.Services.UserService;

public class UserService
{
    private readonly Auth0Client _auth0Client;
    private readonly AuthenticationService _authenticationService;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, Auth0Client auth0Client,
        AuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _auth0Client = auth0Client;
        _authenticationService = authenticationService;
    }

    public async Task<User> RegisterOrUpdateUserAsync()
    {
        var accessToken = _authenticationService.GetAccessTokenFromHttpContext();

        UserInfo userInfo;
        try
        {
            userInfo = await _auth0Client.GetAuth0UserInfoAsync(accessToken);
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

        var user = UserConverter.MapToUser(userInfo);

        await _userRepository.AddOrUpdateAsync(user);

        return user;
    }
}