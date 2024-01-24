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
        if (images.Backdrops.Count == 0)
        {
            return new List<MovieBackdrop>();
        }

        var groupedBackdrops = GroupBackdrops(images.Backdrops);
        return SelectTopImagesForCategories(groupedBackdrops);
    }

    private async Task<MovieImagesResponse> FetchBackdropsFromApiAsync(int movieId)
    {
        var backdropsUri = $"{_baseUrl}movie/{movieId}/images";
        var response = await _httpService.SendAndDeserializeAsync<MovieImagesResponse>(backdropsUri);
        return response ?? new MovieImagesResponse { Backdrops = new List<MovieBackdrop>() };
    }

    private IEnumerable<GroupedBackdrop> GroupBackdrops(IEnumerable<MovieBackdrop> backdrops)
    {
        return backdrops.GroupBy(backdrop => new { backdrop.AspectRatio, backdrop.Width, backdrop.Height })
            .Select(group => new GroupedBackdrop
            {
                AspectRatio = group.Key.AspectRatio,
                Resolution = group.Key.Width * group.Key.Height,
                Backdrops = group.OrderByDescending(b => b.VoteAverage).ToList()
            });
    }

    private List<MovieBackdrop> SelectTopImagesForCategories(IEnumerable<GroupedBackdrop> groupedBackdrops)
    {
        // Materialize the enumerable into a list
        var groupedBackdropsList = groupedBackdrops.ToList();

        var selectedBackdrops = new List<MovieBackdrop>();
        selectedBackdrops.AddRange(SelectTopImagesForCategory(groupedBackdropsList, 1.78)); // Banner
        selectedBackdrops.AddRange(SelectTopImagesForCategory(groupedBackdropsList, 1.0)); // Background
        // Add more categories if needed
        return selectedBackdrops;
    }


    private IEnumerable<MovieBackdrop> SelectTopImagesForCategory(IEnumerable<GroupedBackdrop> groupedBackdrops,
        double desiredAspectRatio)
    {
        return groupedBackdrops
            .Where(group => Math.Abs(group.AspectRatio - desiredAspectRatio) < 0.1)
            .OrderByDescending(group => group.Resolution)
            .SelectMany(group => group.Backdrops)
            .Take(1) // Or more if needed
            .Select(backdrop => new MovieBackdrop
            {
                FilePath = backdrop.FilePath,
                VoteAverage = backdrop.VoteAverage
            });
    }
}