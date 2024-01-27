using HtmlAgilityPack;

namespace MoviesBE.Services.Common;

public abstract class BaseScrapingService
{
    protected readonly HttpClient HttpClient;

    protected BaseScrapingService(HttpClient httpClient)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    protected async Task<HtmlDocument> FetchHtmlDocumentAsync(string url)
    {
        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var pageContent = await response.Content.ReadAsStringAsync();

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(pageContent);

        return htmlDocument;
    }
}