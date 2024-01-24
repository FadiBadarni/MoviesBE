using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDriver _neo4JDriver;

    public MovieRepository(IDriver neo4JDriver)
    {
        _neo4JDriver = neo4JDriver;
    }

    public async Task SaveMovieAsync(Movie movie)
    {
        await using var session = _neo4JDriver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await SaveMovieNodeAsync(movie, tx);
                if (movie.Genres != null)
                {
                    await SaveGenresAsync(movie, tx);
                }

                if (movie.ProductionCompanies != null)
                {
                    await SaveProductionCompaniesAsync(movie, tx);
                }

                if (movie.ProductionCountries != null)
                {
                    await SaveProductionCountriesAsync(movie, tx);
                }

                if (movie.SpokenLanguages != null)
                {
                    await SaveSpokenLanguagesAsync(movie, tx);
                }

                await SaveMovieBackdropsAsync(movie, tx);
            });
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    public async Task<Movie?> GetMovieByIdAsync(int movieId)
    {
        await using var session = _neo4JDriver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie {id: $id})
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (m)-[:PRODUCED_BY]->(c:Company)
                OPTIONAL MATCH (m)-[:PRODUCED_IN]->(pc:Country)
                OPTIONAL MATCH (m)-[:HAS_LANGUAGE]->(sl:Language)
                OPTIONAL MATCH (m)-[:HAS_BACKDROP]->(b:Backdrop)
                RETURN m, COLLECT(DISTINCT g) as genres, COLLECT(DISTINCT c) as companies,
                           COLLECT(DISTINCT pc) as countries, COLLECT(DISTINCT sl) as languages,
                           COLLECT(DISTINCT b) as backdrops",
                new { id = movieId });

            if (await cursor.FetchAsync())
            {
                var movieNode = cursor.Current["m"].As<INode>();
                var genres = cursor.Current["genres"].As<List<INode>>()
                    .Select(GenreNodeConverter.ConvertNodeToGenre).ToList();
                var companies = cursor.Current["companies"].As<List<INode>>()
                    .Select(CompanyNodeConverter.ConvertNodeToCompany).ToList();
                var countries = cursor.Current["countries"].As<List<INode>>()
                    .Select(CountryNodeConverter.ConvertNodeToCountry).ToList();
                var languages = cursor.Current["languages"].As<List<INode>>()
                    .Select(LanguageNodeConverter.ConvertNodeToLanguage).ToList();

                const double bannerAspectRatio = 1.78;
                var backdrops = cursor.Current["backdrops"].As<List<INode>>()
                    .Select(BackdropNodeConverter.ConvertNodeToBackdrop)
                    .OrderByDescending(b =>
                        Math.Abs(b.AspectRatio - bannerAspectRatio)) // Prioritize banner-like aspect ratio
                    .ThenByDescending(b => b.Width * b.Height) // Then by size
                    .ToList();

                var movie = MovieNodeConverter.ConvertNodeToMovie(movieNode);
                movie.Genres = genres;
                movie.ProductionCompanies = companies;
                movie.ProductionCountries = countries;
                movie.SpokenLanguages = languages;
                movie.Backdrops = backdrops;

                return movie;
            }

            return null;
        });
    }


    public async Task<List<PopularMovie>> GetCachedPopularMoviesAsync()
    {
        var movies = new List<PopularMovie>();
        await using var session = _neo4JDriver.AsyncSession();
        const double popularityThreshold = 50;
        try
        {
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (m:Movie)
                          WHERE m.popularity >= $popularityThreshold
                          RETURN m",
                    new { popularityThreshold });

                var records = await cursor.ToListAsync();
                return records;
            });

            foreach (var record in result)
            {
                var movieNode = record["m"].As<INode>();
                var movie = PopularMovieNodeConverter.ConvertNodeToPopularMovie(movieNode);
                movies.Add(movie);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            // Log or rethrow depending on your strategy
        }

        return movies;
    }

    private static async Task SaveMovieNodeAsync(Movie movie, IAsyncQueryRunner tx)
    {
        await tx.RunAsync(
            @"MERGE (m:Movie {id: $id})
          ON CREATE SET
            m.title = $title,
            m.releaseDate = $releaseDate,
            m.overview = $overview,
            m.adult = $adult,
            m.backdropPath = $backdropPath,
            m.budget = $budget,
            m.homepage = $homepage,
            m.imdbId = $imdbId,
            m.originalLanguage = $originalLanguage,
            m.originalTitle = $originalTitle,
            m.popularity = $popularity,
            m.posterPath = $posterPath,
            m.revenue = $revenue,
            m.runtime = $runtime,
            m.status = $status,
            m.tagline = $tagline,
            m.video = $video,
            m.voteAverage = $voteAverage,
            m.voteCount = $voteCount
          ON MATCH SET
            m.title = $title,
            m.releaseDate = $releaseDate,
            m.overview = $overview,
            m.adult = $adult,
            m.backdropPath = $backdropPath,
            m.budget = $budget,
            m.homepage = $homepage,
            m.imdbId = $imdbId,
            m.originalLanguage = $originalLanguage,
            m.originalTitle = $originalTitle,
            m.popularity = $popularity,
            m.posterPath = $posterPath,
            m.revenue = $revenue,
            m.runtime = $runtime,
            m.status = $status,
            m.tagline = $tagline,
            m.video = $video,
            m.voteAverage = $voteAverage,
            m.voteCount = $voteCount",
            new
            {
                id = movie.Id,
                title = movie.Title,
                releaseDate = movie.ReleaseDate,
                overview = movie.Overview,
                adult = movie.Adult,
                backdropPath = movie.BackdropPath,
                budget = movie.Budget,
                homepage = movie.Homepage,
                imdbId = movie.ImdbId,
                originalLanguage = movie.OriginalLanguage,
                originalTitle = movie.OriginalTitle,
                popularity = movie.Popularity,
                posterPath = movie.PosterPath,
                revenue = movie.Revenue,
                runtime = movie.Runtime,
                status = movie.Status,
                tagline = movie.Tagline,
                video = movie.Video,
                voteAverage = movie.VoteAverage,
                voteCount = movie.VoteCount
            });
    }


    private static async Task SaveGenresAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.Genres == null)
        {
            return;
        }

        // Remove existing genre relationships from this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:HAS_GENRE]->(g:Genre)
          DELETE r",
            new { movieId = movie.Id });

        // Merge each genre and create a new relationship with the movie.
        foreach (var genre in movie.Genres)
            await tx.RunAsync(
                @"MERGE (g:Genre {id: $id})
              ON CREATE SET g.name = $name
              ON MATCH SET g.name = $name
              WITH g
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:HAS_GENRE]->(g)",
                new { id = genre.Id, name = genre.Name, movieId = movie.Id });
    }


    private static async Task SaveProductionCompaniesAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.ProductionCompanies == null)
        {
            return;
        }

        // First, detach all existing production company relationships from this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:PRODUCED_BY]->(c:Company)
          DELETE r",
            new { movieId = movie.Id });

        // Then, merge each production company and create a relationship with the movie.
        foreach (var company in movie.ProductionCompanies)
            await tx.RunAsync(
                @"MERGE (c:Company {id: $id})
              ON CREATE SET c.name = $name, c.logoPath = $logoPath, c.originCountry = $originCountry
              ON MATCH SET c.name = $name, c.logoPath = $logoPath, c.originCountry = $originCountry
              WITH c
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:PRODUCED_BY]->(c)",
                new
                {
                    id = company.Id, name = company.Name, logoPath = company.LogoPath,
                    originCountry = company.OriginCountry, movieId = movie.Id
                });
    }


    private static async Task SaveProductionCountriesAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.ProductionCountries == null)
        {
            return;
        }

        // First, detach all existing production country relationships from this movie.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:PRODUCED_IN]->(c:Country)
          DELETE r",
            new { movieId = movie.Id });

        // Then, merge each production country and create a relationship with the movie.
        foreach (var country in movie.ProductionCountries)
            await tx.RunAsync(
                @"MERGE (c:Country {iso31661: $iso31661})
              ON CREATE SET c.name = $name
              ON MATCH SET c.name = $name
              WITH c
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:PRODUCED_IN]->(c)",
                new { iso31661 = country.Iso31661, name = country.Name, movieId = movie.Id });
    }


    private static async Task SaveSpokenLanguagesAsync(Movie movie, IAsyncQueryRunner tx)
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


    private static async Task SaveMovieBackdropsAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.Backdrops == null)
        {
            return;
        }

        // First, detach all existing backdrops from this movie to avoid duplicates.
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:HAS_BACKDROP]->(b:Backdrop)
          DELETE r",
            new { movieId = movie.Id });

        // Then, merge each backdrop and create a relationship with the movie.
        foreach (var backdrop in movie.Backdrops.Where(b => !string.IsNullOrEmpty(b.FilePath)))
            await tx.RunAsync(
                @"MERGE (b:Backdrop {filePath: $filePath})
              ON CREATE SET 
                b.voteAverage = $voteAverage,
                b.aspectRatio = $aspectRatio,
                b.width = $width,
                b.height = $height
              ON MATCH SET 
                b.voteAverage = $voteAverage,
                b.aspectRatio = $aspectRatio,
                b.width = $width,
                b.height = $height
              WITH b
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:HAS_BACKDROP]->(b)",
                new
                {
                    filePath = backdrop.FilePath,
                    voteAverage = backdrop.VoteAverage,
                    aspectRatio = backdrop.AspectRatio,
                    width = backdrop.Width,
                    height = backdrop.Height,
                    movieId = movie.Id
                });
    }
}