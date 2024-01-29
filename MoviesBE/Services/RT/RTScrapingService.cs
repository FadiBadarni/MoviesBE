using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using MoviesBE.Exceptions;
using MoviesBE.Services.Common;

namespace MoviesBE.Services.RT;

public class RTScrapingService : BaseScrapingService
{
    public RTScrapingService(HttpClient httpClient) : base(httpClient) { }

    public async Task<double> GetRottenTomatoesRatingAsync(string movieTitle, string releaseYear = null)
    {
        // First, try to find the movie page URL using the search functionality
        var moviePageUrl = await FindMoviePageUrlAsync(movieTitle, releaseYear);
        if (string.IsNullOrEmpty(moviePageUrl))
        {
            throw new ResourceNotFoundException($"Rotten Tomatoes page not found for {movieTitle}");
        }

        // Then, scrape the movie page for the rating
        var response = await HttpClient.GetAsync(moviePageUrl);
        if (response.IsSuccessStatusCode)
        {
            var pageContent = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

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

        throw new ResourceNotFoundException($"Rotten Tomatoes rating not found for {movieTitle}");
    }

    private async Task<string> FindMoviePageUrlAsync(string movieTitle, string releaseYear)
    {
        var searchQuery = HttpUtility.UrlEncode(movieTitle);
        var searchUrl = $"https://www.rottentomatoes.com/search?search={searchQuery}";
        var response = await HttpClient.GetAsync(searchUrl);

        if (response.IsSuccessStatusCode)
        {
            var pageContent = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            // Parsing logic based on the provided HTML structure
            var movieNodes = doc.DocumentNode.SelectNodes("//search-page-media-row[@releaseyear]");
            if (movieNodes != null)
            {
                foreach (var node in movieNodes)
                {
                    var titleNode = node.SelectSingleNode(".//a[@data-qa='info-name']");
                    var yearAttribute = node.GetAttributeValue("releaseyear", null);

                    if (titleNode != null && yearAttribute != null)
                    {
                        var normalizedTitleFromRT = NormalizeTitle(titleNode.InnerText);
                        var normalizedMovieTitle = NormalizeTitle(movieTitle);
                        var year = yearAttribute.Trim();

                        if (normalizedTitleFromRT.Equals(normalizedMovieTitle) &&
                            (releaseYear == null || year.Equals(releaseYear)))
                        {
                            var relativeUrl = titleNode.GetAttributeValue("href", null);
                            if (!string.IsNullOrEmpty(relativeUrl))
                            {
                                return relativeUrl; // Assuming the href attribute contains the full URL
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    private string NormalizeTitle(string title)
    {
        // Decode HTML entities
        title = WebUtility.HtmlDecode(title);

        // Replace specific characters with their plain text equivalents
        title = title.Replace("&", "and")
            .Replace("'", "");

        // Remove all non-alphanumeric characters (except spaces)
        title = Regex.Replace(title, "[^a-zA-Z0-9 ]", "");

        // Replace multiple spaces with a single space and convert to lower case
        return Regex.Replace(title, @"\s+", " ").Trim().ToLowerInvariant();
    }
}