﻿using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class MLanguageRepository : IMLanguageRepository
{
    public async Task SaveSpokenLanguagesAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.SpokenLanguages == null)
        {
            return;
        }

        // First, detach all existing spoken language relationships from this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:HAS_LANGUAGE]->(l:Language)
          DELETE r",
            new { movieId = movie.Id });

        // Then, merge each spoken language and create a relationship with the movie.
        foreach (var language in movie.SpokenLanguages)
            await tx.RunAsync(
                @"MERGE (l:Language {iso6391: $iso6391})
              ON CREATE SET l.name = $name, l.englishName = $englishName
              ON MATCH SET l.name = $name, l.englishName = $englishName
              WITH l
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:HAS_LANGUAGE]->(l)",
                new
                {
                    iso6391 = language.Iso6391,
                    name = language.Name,
                    englishName = language.EnglishName,
                    movieId = movie.Id
                });
    }

    public async Task<List<SpokenLanguage>> GetMovieSpokenLanguagesAsync(IAsyncQueryRunner tx, int movieId)
    {
        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie)-[:HAS_LANGUAGE]->(sl:Language) WHERE m.id = $id RETURN COLLECT(DISTINCT sl) as languages",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["languages"].As<List<INode>>()
                .Select(LanguageNodeConverter.ConvertNodeToLanguage).ToList();
        }

        return new List<SpokenLanguage>();
    }
}