using MoviesBE.Entities;
using MoviesBE.Exceptions;
using MoviesBE.Services.Common;
using MoviesBE.Services.Factories;
using MoviesBE.Utilities.Scrape;

namespace MoviesBE.Services.RT;

public class RTRatingUpdateService : BaseHostedService
{
    private readonly MovieRepositoryFactory _movieRepositoryFactory;
    private readonly RatingRepositoryFactory _ratingRepositoryFactory;
    private readonly RTScrapingServiceFactory _rtScrapingServiceFactory;

    public RTRatingUpdateService(
        IConfiguration configuration,
        ILogger<RTRatingUpdateService> logger,
        RTScrapingServiceFactory rtScrapingServiceFactory,
        MovieRepositoryFactory movieRepositoryFactory,
        RatingRepositoryFactory ratingRepositoryFactory)
        : base(configuration, logger, "RTScraper")
    {
        _rtScrapingServiceFactory = rtScrapingServiceFactory;
        _movieRepositoryFactory = movieRepositoryFactory;
        _ratingRepositoryFactory = ratingRepositoryFactory;
    }

    protected override void ExecuteTask(object? state)
    {
        if (!IsEnabled)
        {
            Logger.LogInformation("RT Rating Update Service is disabled, exiting update loop.");
            return;
        }

        Task.Run(async () =>
        {
            var movieRepository = _movieRepositoryFactory.Create();
            var movies = await movieRepository.GetMoviesWithoutRTRatingAsync();
            var delayConfig = Configuration.GetValue<int>("RTUpdate:DelayMilliseconds");

            foreach (var movie in movies)
            {
                if (StoppingToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Cancellation requested, stopping RT Rating Update Service.");
                    break;
                }

                // Generate the RT formatted title from the movie title
                var formattedTitle = movie.Title;
                if (string.IsNullOrWhiteSpace(formattedTitle))
                {
                    Logger.LogInformation(
                        $"Skipping movie ID {movie.Id} as formatted RT title could not be generated.");
                    continue;
                }

                try
                {
                    var rtScrapingService = _rtScrapingServiceFactory.Create();
                    var year = movie.ReleaseDate?.Length >= 4 ? movie.ReleaseDate.Substring(0, 4) : null;
                    var rating = await rtScrapingService.GetRottenTomatoesRatingAsync(formattedTitle, year);

                    if (rating > 0)
                    {
                        var normalizedRating = rating / 10;

                        var ratingRepository = _ratingRepositoryFactory.Create();
                        await ratingRepository.UpdateMovieRatingsAsync(movie.Id,
                            new List<Rating> { new() { Provider = "RottenTomatoes", Score = normalizedRating } });
                    }
                    else
                    {
                        Logger.LogWarning($"No valid RT rating found for movie ID {movie.Id}.");
                    }
                }
                catch (ResourceNotFoundException ex)
                {
                    Logger.LogWarning(ex, $"Rotten Tomatoes page not found for movie ID {movie.Id}.");
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInformation("Operation canceled during RT rating update.");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error updating RT rating for movie ID {movie.Id}");
                }

                await Task.Delay(DelayUtility.GetRandomizedDelay(delayConfig), StoppingToken);
            }
        }, StoppingToken).ConfigureAwait(false);
    }


    protected override TimeSpan GetInterval()
    {
        var intervalHours = Configuration.GetSection("RTScraper").GetValue<int>("IntervalHours");
        return TimeSpan.FromHours(intervalHours);
    }
}