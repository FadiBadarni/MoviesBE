﻿using System.Security.Cryptography;
using MoviesBE.Entities;
using MoviesBE.Services.Factories;

namespace MoviesBE.Services.IMDB;

public class IMDbRatingUpdateService : IHostedService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IMDbScrapingServiceFactory _imdbScrapingServiceFactory;
    private readonly bool _isEnabled;
    private readonly ILogger<IMDbRatingUpdateService> _logger;
    private readonly MovieRepositoryFactory _movieRepositoryFactory;
    private readonly RatingRepositoryFactory _ratingRepositoryFactory;
    private CancellationToken _stoppingToken;
    private Timer? _timer;

    public IMDbRatingUpdateService(ILogger<IMDbRatingUpdateService> logger,
        IMDbScrapingServiceFactory imdbScrapingServiceFactory,
        MovieRepositoryFactory movieRepositoryFactory,
        RatingRepositoryFactory ratingRepositoryFactory, IConfiguration configuration)
    {
        _logger = logger;
        _imdbScrapingServiceFactory = imdbScrapingServiceFactory;
        _movieRepositoryFactory = movieRepositoryFactory;
        _ratingRepositoryFactory = ratingRepositoryFactory;
        _configuration = configuration;
        _isEnabled = configuration.GetValue<bool>("IMDbUpdate:Enabled");
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IMDb Rating Update Service is starting.");
        _stoppingToken = cancellationToken;

        if (_isEnabled)
        {
            _logger.LogInformation("IMDb Rating Update Service is enabled and will run.");
            _timer = new Timer(UpdateRatings, null, TimeSpan.Zero, TimeSpan.FromHours(24));
        }
        else
        {
            _logger.LogInformation("IMDb Rating Update Service is disabled and will not run.");
        }

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
        if (!_isEnabled)
        {
            _logger.LogInformation("IMDb Rating Update Service is disabled, exiting update loop.");
            return;
        }

        var movieRepository = _movieRepositoryFactory.Create();
        var movies = await movieRepository.GetMoviesWithoutIMDbRatingAsync();
        var delayConfig = _configuration.GetValue<int>("IMDbUpdate:DelayMilliseconds");

        foreach (var movie in movies)
        {
            if (_stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested, stopping IMDb Rating Update Service.");
                break;
            }

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
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation canceled during IMDb rating update.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating IMDb rating for movie ID {movie.Id}");
            }

            await Task.Delay(GetRandomizedDelay(delayConfig), _stoppingToken);
        }
    }

    private int GetRandomizedDelay(int baseDelay)
    {
        var jitterRange = GetJitterRange();
        using var rng = RandomNumberGenerator.Create();
        var jitterBytes = new byte[4];
        rng.GetBytes(jitterBytes);
        var jitter = BitConverter.ToInt32(jitterBytes, 0);

        // Ensure jitter is non-negative and within the jitter range
        jitter = Math.Abs(jitter % jitterRange);

        // Calculate the total delay, ensuring it's within the acceptable range
        var totalDelay = Math.Max(0, baseDelay + jitter - jitterRange / 2);

        // Ensure the delay is within the acceptable range for Task.Delay
        return Math.Min(totalDelay, int.MaxValue);
    }

    private int GetJitterRange()
    {
        // Dynamically determine the jitter range
        var hour = DateTime.Now.Hour;
        return hour >= 8 && hour <= 18 ? 10000 : 5000; // Peak hours have a larger range
    }
}