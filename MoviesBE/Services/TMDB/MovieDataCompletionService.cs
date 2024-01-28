using MoviesBE.Services.Common;
using MoviesBE.Services.Factories;

namespace MoviesBE.Services.TMDB;

public class MovieDataCompletionService : BaseHostedService
{
    private readonly MovieRepositoryFactory _movieRepositoryFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MovieDataCompletionService(
        IConfiguration configuration,
        ILogger<MovieDataCompletionService> logger,
        MovieRepositoryFactory movieRepositoryFactory,
        IServiceScopeFactory serviceScopeFactory)
        : base(configuration, logger, "MovieDataCompletion")
    {
        _movieRepositoryFactory = movieRepositoryFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override void ExecuteTask(object? state)
    {
        if (!IsEnabled)
        {
            Logger.LogInformation("Movie Data Completion Service is disabled, exiting.");
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var movieDataService = scope.ServiceProvider.GetRequiredService<MovieDataService>();
                    var movieRepository = _movieRepositoryFactory.Create();
                    var movies = await movieRepository.GetAllMoviesAsync();

                    foreach (var movie in movies)
                    {
                        if (StoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!movieDataService.IsMovieDataComplete(movie))
                        {
                            await movieDataService.GetMovieAsync(movie.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in Movie Data Completion Service.");
            }
        }, StoppingToken).ConfigureAwait(false);
    }

    protected override TimeSpan GetInterval()
    {
        var intervalHours = Configuration.GetSection("MovieDataCompletion").GetValue<int>("IntervalHours");
        return TimeSpan.FromHours(intervalHours);
    }
}