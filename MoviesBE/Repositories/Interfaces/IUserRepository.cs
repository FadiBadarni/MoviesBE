using MoviesBE.Entities;

namespace MoviesBE.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByAuth0IdAsync(string auth0Id);

    Task AddOrUpdateAsync(User user);
    Task<bool> BookmarkMovieAsync(string userId, int movieId);
    Task<List<int>> FetchWatchlistAsync(string userId);
    Task<bool> UnbookmarkMovieAsync(string userId, int movieId);
}