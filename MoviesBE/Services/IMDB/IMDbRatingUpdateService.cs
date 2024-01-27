using MoviesBE.Entities;
using MoviesBE.Services.Factories;

namespace MoviesBE.Services.IMDB;

public class IMDbRatingUpdateService : IHostedService, IDisposable
{
    private readonly IMDbScrapingServiceFactory _imdbScrapingServiceFactory;
    private readonly ILogger<IMDbRatingUpdateService> _logger;
    private readonly MovieRepositoryFactory _movieRepositoryFactory;
    private readonly RatingRepositoryFactory _ratingRepositoryFactory;
    private Timer? _timer;

    public IMDbRatingUpdateService(ILogger<IMDbRatingUpdateService> logger,
        IMDbScrapingServiceFactory imdbScrapingServiceFactory,
        MovieRepositoryFactory movieRepositoryFactory,
        RatingRepositoryFactory ratingRepositoryFactory)
    {
        _logger = logger;
        _imdbScrapingServiceFactory = imdbScrapingServiceFactory;
        _movieRepositoryFactory = movieRepositoryFactory;
        _ratingRepositoryFactory = ratingRepositoryFactory;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IMDb Rating Update Service running.");

        _timer = new Timer(UpdateRatings, null, TimeSpan.Zero,
            TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IMDb Rating Update Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void UpdateRatings(object? state)
    {
        var movieRepository = _movieRepositoryFactory.Create();
        var movies = await movieRepository.GetMoviesWithoutIMDbRatingAsync();

        foreach (var movie in movies)
        {
            if (string.IsNullOrWhiteSpace(movie.ImdbId))
            {
                _logger.LogInformation($"Skipping movie ID {movie.Id} as IMDb ID is missing.");
                continue; // Skip movies with no IMDb ID
            }

            try
            {
                var imdbScrapingService = _imdbScrapingServiceFactory.Create();
                var rating = await imdbScrapingService.GetIMDbRatingAsync(movie.ImdbId);

                // Update the rating if a valid value is returned, or if the existing rating is 0
                if (rating > 0 || (rating == 0 && movie.Ratings.Any(r => r.Provider == "IMDb" && r.Score == 0)))
                {
                    var ratingRepository = _ratingRepositoryFactory.Create();
                    await ratingRepository.UpdateMovieRatingsAsync(movie.Id,
                        new List<Rating> { new() { Provider = "IMDb", Score = rating } });
                }
                else
                {
                    _logger.LogWarning($"No valid IMDb rating found for movie ID {movie.Id}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating IMDb rating for movie ID {movie.Id}");
            }

            await Task.Delay(10000);
        }
    }
}