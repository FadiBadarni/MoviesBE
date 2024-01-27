using MoviesBE.Repositories.Interfaces;

namespace MoviesBE.Services.Factories;

public class RatingRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RatingRepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRatingRepository Create()
    {
        // Create a new scope and resolve the service
        return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IRatingRepository>();
    }
}