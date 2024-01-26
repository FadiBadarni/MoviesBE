using MoviesBE.Entities;

namespace MoviesBE.Services.IMDB;

public class IMDbRatingUpdateService : IHostedService, IDisposable
{
    private readonly IMDbScrapingServiceFactory _imdbScrapingServiceFactory;
    private readonly ILogger<IMDbRatingUpdateService> _logger;
    private readonly MovieRepositoryFactory _movieRepositoryFactory;
    private readonly RatingRepositoryFactory _ratingRepositoryFactory;
    private Timer _timer;

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
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IMDb Rating Update Service running.");

        _timer = new Timer(UpdateRatings, null, TimeSpan.Zero,
            TimeSpan.FromHours(24)); // Adjust interval as needed

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IMDb Rating Update Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void UpdateRatings(object state)
    {
        var movieRepository = _movieRepositoryFactory.Create();
        var movies = await movieRepository.GetMoviesWithoutIMDbRatingAsync();

        foreach (var movie in movies)
        {
            try
            {
                // Use the factory to create an instance of IMDbScrapingService
                var imdbScrapingService = _imdbScrapingServiceFactory.Create();

                // Now use this instance to call GetIMDbRatingAsync
                var rating = await imdbScrapingService.GetIMDbRatingAsync(movie.ImdbId);

                if (rating != null)
                {
                    // Use the RatingRepositoryFactory to create an instance of IRatingRepository
                    var ratingRepository = _ratingRepositoryFactory.Create();

                    // Save the updated movie ratings
                    await ratingRepository.UpdateMovieRatingsAsync(movie.Id,
                        new List<Rating> { new() { Provider = "IMDb", Score = rating } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating IMDb rating for movie ID {movie.Id}");
            }

            await Task.Delay(10000); // Delay between requests to avoid rate limiting
        }
    }
}