namespace MoviesBE.Services.IMDB;

public class IMDbScrapingServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IMDbScrapingServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IMDbScrapingService Create()
    {
        // Create a new scope and resolve the service
        return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IMDbScrapingService>();
    }
}