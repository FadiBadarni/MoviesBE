using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IMLanguageRepository
{
    Task SaveSpokenLanguagesAsync(Movie movie, IAsyncQueryRunner tx);
}