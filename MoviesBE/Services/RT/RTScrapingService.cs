using HtmlAgilityPack;
using MoviesBE.Exceptions;
using MoviesBE.Services.Common;

namespace MoviesBE.Services.RT;

public class RTScrapingService : BaseScrapingService
{
    public RTScrapingService(HttpClient httpClient) : base(httpClient) { }

    public async Task<double> GetRottenTomatoesRatingAsync(string movieTitle, string releaseYear = null)
    {
        var formattedTitle = movieTitle.ToLowerInvariant().Replace(" ", "_");
        var urlsToTry = new List<string>
        {
            $"https://www.rottentomatoes.com/m/{formattedTitle}_{releaseYear}", // Title with Year
            $"https://www.rottentomatoes.com/m/{formattedTitle}" // Title only
        };

        foreach (var url in urlsToTry)
        {
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var pageContent = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(pageContent);

                // Extract the year from the page to verify it matches the movie's release year
                var infoNode = doc.DocumentNode.SelectSingleNode("//p[@class='info']");
                if (infoNode != null && !string.IsNullOrEmpty(releaseYear) && infoNode.InnerText.Contains(releaseYear))
                {
                    var scoreBoardNode = doc.DocumentNode.SelectSingleNode("//score-board-deprecated");
                    if (scoreBoardNode != null)
                    {
                        var tomatometerScoreAttribute = scoreBoardNode.GetAttributeValue("tomatometerscore", null);
                        if (tomatometerScoreAttribute != null &&
                            double.TryParse(tomatometerScoreAttribute, out var rating))
                        {
                            return rating;
                        }
                    }
                }
            }
        }

        throw new ResourceNotFoundException($"Rotten Tomatoes page not found for {movieTitle}");
    }
}