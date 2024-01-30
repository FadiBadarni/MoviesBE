using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;

namespace MoviesBE.Services.Graph;

public class GraphDataService
{
    private readonly IMovieRepository _movieRepository;

    public GraphDataService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<Movie>> GetMoviesByGenreAsync(int genreId)
    {
        return await _movieRepository.GetMoviesByGenreAsync(genreId);
    }
}