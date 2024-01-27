using MoviesBE.Entities;
using MoviesBE.Services.Common;
using MoviesBE.Services.Factories;
using MoviesBE.Utilities.Scrape;

namespace MoviesBE.Services.IMDB;

public class IMDbRatingUpdateService : BaseHostedService
{
    private readonly IMDbScrapingServiceFactory _imdbScrapingServiceFactory;
    private readonly MovieRepositoryFactory _movieRepositoryFactory;
    private readonly RatingRepositoryFactory _ratingRepositoryFactory;

    public IMDbRatingUpdateService(
        IConfiguration configuration,
        ILogger<IMDbRatingUpdateService> logger,
        IMDbScrapingServiceFactory imdbScrapingServiceFactory,
        MovieRepositoryFactory movieRepositoryFactory,
        RatingRepositoryFactory ratingRepositoryFactory)
        : base(configuration, logger, "IMDbScraper")
    {
        _imdbScrapingServiceFactory = imdbScrapingServiceFactory;
        _movieRepositoryFactory = movieRepositoryFactory;
        _ratingRepositoryFactory = ratingRepositoryFactory;
    }

    protected override void ExecuteTask(object? state)
    {
        if (!IsEnabled)
        {
            Logger.LogInformation("IMDb Rating Update Service is disabled, exiting update loop.");
            return;
        }

        Task.Run(async () =>
        {
            var movieRepository = _movieRepositoryFactory.Create();
            var movies = await movieRepository.GetMoviesWithoutIMDbRatingAsync();
            var delayConfig = Configuration.GetValue<int>("IMDbUpdate:DelayMilliseconds");

            foreach (var movie in movies)
            {
                if (StoppingToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Cancellation requested, stopping IMDb Rating Update Service.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(movie.ImdbId))
                {
                    Logger.LogInformation($"Skipping movie ID {movie.Id} as IMDb ID is missing.");
                    continue;
                }

                try
                {
                    var imdbScrapingService = _imdbScrapingServiceFactory.Create();
                    var rating = await imdbScrapingService.GetIMDbRatingAsync(movie.ImdbId);

                    if (rating > 0)
                    {
                        var ratingRepository = _ratingRepositoryFactory.Create();
                        await ratingRepository.UpdateMovieRatingsAsync(movie.Id,
                            new List<Rating> { new() { Provider = "IMDb", Score = rating } });
                    }
                    else
                    {
                        Logger.LogWarning($"No valid IMDb rating found for movie ID {movie.Id}.");
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInformation("Operation canceled during IMDb rating update.");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error updating IMDb rating for movie ID {movie.Id}");
                }

                await Task.Delay(DelayUtility.GetRandomizedDelay(delayConfig), StoppingToken);
            }
        }, StoppingToken).ConfigureAwait(false);
    }


    protected override TimeSpan GetInterval()
    {
        var intervalHours = Configuration.GetSection("IMDbScraper").GetValue<int>("IntervalHours");
        return TimeSpan.FromHours(intervalHours);
    }
}