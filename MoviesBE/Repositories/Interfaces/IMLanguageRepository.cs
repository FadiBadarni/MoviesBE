using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IMLanguageRepository
{
    Task SaveSpokenLanguagesAsync(Movie movie, IAsyncQueryRunner tx);
    Task<List<SpokenLanguage>> GetMovieSpokenLanguagesAsync(IAsyncQueryRunner tx, int movieId);
}