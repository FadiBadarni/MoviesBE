using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IPCompanyRepository
{
    Task SaveProductionCompaniesAsync(Movie movie, IAsyncQueryRunner tx);
}