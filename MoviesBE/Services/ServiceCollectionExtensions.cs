using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MoviesBE.Repositories.Implementations;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Services.Database;
using MoviesBE.Services.Factories;
using MoviesBE.Services.IMDB;
using MoviesBE.Services.RT;
using MoviesBE.Services.TMDB;
using MoviesBE.Services.UserService;
using Neo4j.Driver;

namespace MoviesBE.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNeo4jServices(this IServiceCollection services, IConfiguration configuration)
    {
        var neo4JConfig = configuration.GetSection("Neo4j");

        services.AddSingleton(GraphDatabase.Driver(neo4JConfig["Uri"],
            AuthTokens.Basic(neo4JConfig["Username"], neo4JConfig["Password"])));

        // Repository registrations
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<ICreditsRepository, CreditsRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IPCompanyRepository, PCompanyRepository>();
        services.AddScoped<IPCountryRepository, PCountryRepository>();
        services.AddScoped<IMLanguageRepository, MLanguageRepository>();
        services.AddScoped<IMBackdropRepository, MBackdropRepository>();
        services.AddScoped<IMVideoRepository, MVideoRepository>();
        services.AddScoped<IMovieCollectionRepository, MovieCollectionRepository>();
        services.AddScoped<IPaginationTrackerRepository, PaginationTrackerRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddSingleton<Neo4JService>();

        return services;
    }

    public static IServiceCollection AddMovieServices(this IServiceCollection services)
    {
        // Factories
        services.AddSingleton<MovieRepositoryFactory>();
        services.AddSingleton<RatingRepositoryFactory>();

        services.AddScoped<MovieDataService>();
        services.AddScoped<TmdbApiService>();
        services.AddScoped<MovieBackdropService>();
        services.AddScoped<MovieVideoOrganizerService>();
        services.AddScoped<CrewFilterService>();
        services.AddScoped<PopularityThresholdService>();
        services.AddScoped<RatingThresholdService>();

        services.AddHostedService<IMDbRatingUpdateService>();
        services.AddSingleton<IMDbScrapingServiceFactory>();
        services.AddScoped<IMDbScrapingService>();

        services.AddHostedService<RTRatingUpdateService>();
        services.AddSingleton<RTScrapingServiceFactory>();
        services.AddScoped<RTScrapingService>();

        services.AddHostedService<MovieDataCompletionService>();

        services.AddHttpClient<HttpService>();

        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AuthenticationService>();
        services.AddHttpClient<Auth0Client>();

        services.AddScoped<UserService.UserService>();
        services.AddScoped<RecommendationService>();
        services.AddScoped<IUserRepository, UserRepository>();

        var auth0Settings = configuration.GetSection("Auth0");
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://{auth0Settings["Domain"]}/";
                options.Audience = auth0Settings["Audience"];
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        return services;
    }
}