using HtmlAgilityPack;

namespace MoviesBE.Services.IMDB;

public class IMDbScrapingService
{
    private readonly HttpClient _httpClient;

    public IMDbScrapingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<double> GetIMDbRatingAsync(string imdbId)
    {
        var url = $"https://www.imdb.com/title/{imdbId}/";
        var response = await _httpClient.GetAsync(url);
        var pageContent = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        var ratingNode =
            doc.DocumentNode.SelectSingleNode("//div[@data-testid='hero-rating-bar__aggregate-rating__score']/span");

        // Handle the case where the rating might not be available or is invalid
        if (ratingNode == null || !double.TryParse(ratingNode.InnerText.Trim(), out var rating))
        {
            return 0;
        }

        return rating;
    }
}