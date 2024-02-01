using MoviesBE.Entities;

namespace MoviesBE.Repositories.Implementations;

public interface IUserRepository
{
    Task<User> FindByAuth0IdAsync(string auth0Id);

    Task UpdateAsync(User user);
    Task AddAsync(User user);
}