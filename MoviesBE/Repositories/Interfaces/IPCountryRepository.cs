using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IPCountryRepository
{
    Task SaveProductionCountriesAsync(Movie movie, IAsyncQueryRunner tx);
    Task<List<ProductionCountry>> GetMovieProductionCountriesAsync(IAsyncQueryRunner tx, int movieId);
}