using MoviesBE.Services.Common;

namespace MoviesBE.Services.IMDB;

public class IMDbScrapingService : BaseScrapingService
{
    public IMDbScrapingService(HttpClient httpClient) : base(httpClient) { }

    public async Task<double> GetIMDbRatingAsync(string imdbId)
    {
        var url = $"https://www.imdb.com/title/{imdbId}/";
        var doc = await FetchHtmlDocumentAsync(url);

        var ratingNode =
            doc.DocumentNode.SelectSingleNode("//div[@data-testid='hero-rating-bar__aggregate-rating__score']/span");

        if (ratingNode == null || !double.TryParse(ratingNode.InnerText.Trim(), out var rating))
        {
            return 0;
        }

        return rating;
    }
}