using MoviesBE.Services.Common;

namespace MoviesBE.Services.RT;

public class RTScrapingService : BaseScrapingService
{
    public RTScrapingService(HttpClient httpClient) : base(httpClient) { }

    public async Task<double> GetRottenTomatoesRatingAsync(string movieTitle)
    {
        // Transform the movie title into the format used by Rotten Tomatoes in URLs
        var formattedTitle = movieTitle.ToLowerInvariant().Replace(" ", "_");
        var url = $"https://www.rottentomatoes.com/m/{formattedTitle}";
        var doc = await FetchHtmlDocumentAsync(url);

        // XPath to locate the rating element
        var scoreBoardNode = doc.DocumentNode.SelectSingleNode("//score-board-deprecated");

        if (scoreBoardNode != null)
        {
            // Extract the tomatometerscore attribute
            var tomatometerScoreAttribute = scoreBoardNode.GetAttributeValue("tomatometerscore", null);
            if (tomatometerScoreAttribute != null && double.TryParse(tomatometerScoreAttribute, out var rating))
            {
                return rating;
            }
        }

        // Return 0 if parsing fails or the node is not found
        return 0;
    }
}