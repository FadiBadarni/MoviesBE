using MoviesBE.Entities;

namespace MoviesBE.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByAuth0IdAsync(string auth0Id);

    Task AddOrUpdateAsync(User user);
}