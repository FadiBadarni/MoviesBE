using MoviesBE.DTOs;
using MoviesBE.Repositories.Interfaces;

namespace MoviesBE.Services.UserService;

public class RecommendationService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUserRepository _userRepository;

    public RecommendationService(IMovieRepository movieRepository, IUserRepository userRepository)
    {
        _movieRepository = movieRepository;
        _userRepository = userRepository;
    }

    public async Task<(List<RecommendedMovie>, int)> GetRecommendedMoviesAsync(string userId, int page, int pageSize,
        string ratingFilter = null, int? genreFilter = null)
    {
        return (new List<RecommendedMovie>(), 0);
    }
}