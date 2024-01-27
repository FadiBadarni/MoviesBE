using MoviesBE.Repositories.Interfaces;

namespace MoviesBE.Services.Factories;

public class MovieRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MovieRepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IMovieRepository Create()
    {
        return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IMovieRepository>();
    }
}