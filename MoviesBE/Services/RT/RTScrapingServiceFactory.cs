namespace MoviesBE.Services.RT;

public class RTScrapingServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RTScrapingServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public RTScrapingService Create()
    {
        // Create a new scope and resolve the service
        return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<RTScrapingService>();
    }
}