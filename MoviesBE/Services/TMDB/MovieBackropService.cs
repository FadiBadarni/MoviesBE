using MoviesBE.DTOs;
using MoviesBE.Entities;

namespace MoviesBE.Services.TMDB;

public class MovieBackdropService
{
    private readonly string _baseUrl;
    private readonly HttpService _httpService;

    public MovieBackdropService(HttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
        _baseUrl = configuration["TMDB:BaseUrl"] ?? throw new InvalidOperationException("Base URL is not configured.");
    }

    public async Task<List<MovieBackdrop>> FetchMovieBackdropsAsync(int movieId)
    {
        var images = await FetchBackdropsFromApiAsync(movieId);
        if (!images.Backdrops.Any())
        {
            return new List<MovieBackdrop>();
        }

        return SelectTopBackdrops(images.Backdrops, 10); // Limit total backdrops to 10
    }

    private async Task<MovieImagesResponse> FetchBackdropsFromApiAsync(int movieId)
    {
        var backdropsUri = $"{_baseUrl}movie/{movieId}/images";
        var response = await _httpService.SendAndDeserializeAsync<MovieImagesResponse>(backdropsUri);
        return response ?? new MovieImagesResponse { Backdrops = new List<MovieBackdrop>() };
    }


    private List<MovieBackdrop> SelectTopBackdrops(IEnumerable<MovieBackdrop> backdrops, int maxTotal)
    {
        const double aspectRatioTolerance = 0.2; // Increased tolerance
        var groupedBackdrops = backdrops
            .GroupBy(b => Math.Round(b.AspectRatio / aspectRatioTolerance) * aspectRatioTolerance)
            .OrderByDescending(group => group.Average(b => b.VoteAverage))
            .ToList();

        var selectedBackdrops = new List<MovieBackdrop>();
        foreach (var group in groupedBackdrops)
        {
            var topBackdropsInGroup =
                group.OrderByDescending(b => b.VoteAverage).Take(maxTotal - selectedBackdrops.Count);
            selectedBackdrops.AddRange(topBackdropsInGroup);

            if (selectedBackdrops.Count >= maxTotal)
            {
                break;
            }
        }

        return selectedBackdrops;
    }
}