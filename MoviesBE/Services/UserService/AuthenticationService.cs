namespace MoviesBE.Services.UserService;

public class AuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetAccessTokenFromHttpContext()
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