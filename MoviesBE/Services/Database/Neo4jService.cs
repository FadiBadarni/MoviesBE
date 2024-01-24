using MoviesBE.Data;
using Neo4j.Driver;

namespace MoviesBE.Services;

public class Neo4JService : IAsyncDisposable
{
    private readonly IDriver _neo4JDriver;

    public Neo4JService(IDriver neo4JDriver)
    {
        _neo4JDriver = neo4JDriver;
    }

    public async ValueTask DisposeAsync()
    {
        await _neo4JDriver.DisposeAsync();
        GC.SuppressFinalize(this);
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
                RETURN m, COLLECT(DISTINCT g) as genres, COLLECT(DISTINCT c) as companies,
                       COLLECT(DISTINCT pc) as countries, COLLECT(DISTINCT sl) as languages",
                new { id = movieId });

            if (await cursor.FetchAsync())
            {
                var movieNode = cursor.Current["m"].As<INode>();
                var genres = cursor.Current["genres"].As<List<INode>>().Select(ConvertNodeToGenre).ToList();
                var companies = cursor.Current["companies"].As<List<INode>>().Select(ConvertNodeToCompany).ToList();
                var countries = cursor.Current["countries"].As<List<INode>>().Select(ConvertNodeToCountry).ToList();
                var languages = cursor.Current["languages"].As<List<INode>>().Select(ConvertNodeToLanguage)
                    .ToList();

                var movie = ConvertNodeToMovie(movieNode);
                movie.Genres = genres;
                movie.ProductionCompanies = companies;
                movie.ProductionCountries = countries;
                movie.SpokenLanguages = languages;

                return movie;
            }

            return null;
        });
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
            m.overview = $overview",
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

        foreach (var genre in movie.Genres)
            await tx.RunAsync(
                @"MERGE (g:Genre {id: $id})
              ON CREATE SET g.name = $name
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

        foreach (var company in movie.ProductionCompanies)
            await tx.RunAsync(
                @"MERGE (c:Company {id: $id})
              ON CREATE SET c.name = $name, c.logoPath = $logoPath, c.originCountry = $originCountry
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

        foreach (var country in movie.ProductionCountries)
            await tx.RunAsync(
                @"MERGE (c:Country {iso31661: $iso31661})
              ON CREATE SET c.name = $name
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

        foreach (var language in movie.SpokenLanguages)
            await tx.RunAsync(
                @"MERGE (l:Language {iso6391: $iso6391})
              ON CREATE SET l.name = $name, l.englishName = $englishName
              WITH l
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:HAS_LANGUAGE]->(l)",
                new
                {
                    iso6391 = language.Iso6391, name = language.Name, englishName = language.EnglishName,
                    movieId = movie.Id
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
                var movie = ConvertNodeToPopularMovie(movieNode);
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

    private static PopularMovie ConvertNodeToPopularMovie(IEntity node)
    {
        return new PopularMovie
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Title = node.Properties.ContainsKey("title") ? node.Properties["title"].As<string>() : string.Empty,
            PosterPath = node.Properties.ContainsKey("posterPath")
                ? node.Properties["posterPath"].As<string>()
                : string.Empty,
            ReleaseDate = node.Properties.ContainsKey("releaseDate")
                ? node.Properties["releaseDate"].As<string>()
                : string.Empty,
            Overview = node.Properties.ContainsKey("overview") ? node.Properties["overview"].As<string>() : string.Empty
        };
    }

    private static Movie ConvertNodeToMovie(IEntity node)
    {
        return new Movie
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Title = node.Properties.ContainsKey("title") ? node.Properties["title"].As<string>() : string.Empty,
            ReleaseDate = node.Properties.ContainsKey("releaseDate")
                ? node.Properties["releaseDate"].As<string>()
                : string.Empty,
            Overview =
                node.Properties.ContainsKey("overview") ? node.Properties["overview"].As<string>() : string.Empty,
            Adult = node.Properties.ContainsKey("adult") ? node.Properties["adult"].As<bool>() : false,
            BackdropPath = node.Properties.ContainsKey("backdropPath")
                ? node.Properties["backdropPath"].As<string>()
                : string.Empty,
            Budget = node.Properties.ContainsKey("budget") ? node.Properties["budget"].As<long>() : 0L,
            Homepage =
                node.Properties.ContainsKey("homepage") ? node.Properties["homepage"].As<string>() : string.Empty,
            ImdbId = node.Properties.ContainsKey("imdbId") ? node.Properties["imdbId"].As<string>() : string.Empty,
            OriginalLanguage = node.Properties.ContainsKey("originalLanguage")
                ? node.Properties["originalLanguage"].As<string>()
                : string.Empty,
            OriginalTitle = node.Properties.ContainsKey("originalTitle")
                ? node.Properties["originalTitle"].As<string>()
                : string.Empty,
            Popularity = node.Properties.ContainsKey("popularity") ? node.Properties["popularity"].As<double>() : 0.0,
            PosterPath = node.Properties.ContainsKey("posterPath")
                ? node.Properties["posterPath"].As<string>()
                : string.Empty,
            Revenue = node.Properties.ContainsKey("revenue") ? node.Properties["revenue"].As<long>() : 0L,
            Runtime = node.Properties.ContainsKey("runtime") ? node.Properties["runtime"].As<int>() : 0,
            Status = node.Properties.ContainsKey("status") ? node.Properties["status"].As<string>() : string.Empty,
            Tagline = node.Properties.ContainsKey("tagline") ? node.Properties["tagline"].As<string>() : string.Empty,
            Video = node.Properties.ContainsKey("video") ? node.Properties["video"].As<bool>() : false,
            VoteAverage =
                node.Properties.ContainsKey("voteAverage") ? node.Properties["voteAverage"].As<double>() : 0.0,
            VoteCount = node.Properties.ContainsKey("voteCount") ? node.Properties["voteCount"].As<int>() : 0
            // Handle complex properties like Genres, ProductionCompanies, etc. here
        };
    }

    private static async Task SaveMovieBackdropsAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.Backdrops == null)
        {
            return;
        }

        foreach (var backdrop in movie.Backdrops.Where(b => !string.IsNullOrEmpty(b.FilePath)))
            await tx.RunAsync(
                @"MERGE (b:Backdrop {filePath: $filePath})
            ON CREATE SET b.voteAverage = $voteAverage
            WITH b
            MATCH (m:Movie {id: $movieId})
            MERGE (m)-[:HAS_BACKDROP]->(b)",
                new
                {
                    filePath = backdrop.FilePath,
                    voteAverage = backdrop.VoteAverage,
                    movieId = movie.Id
                });
    }

    private static Genre ConvertNodeToGenre(IEntity node)
    {
        return new Genre
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : string.Empty
        };
    }

    private static ProductionCompany ConvertNodeToCompany(IEntity node)
    {
        return new ProductionCompany
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : string.Empty,
            LogoPath =
                node.Properties.ContainsKey("logoPath") ? node.Properties["logoPath"].As<string>() : string.Empty,
            OriginCountry = node.Properties.ContainsKey("originCountry")
                ? node.Properties["originCountry"].As<string>()
                : string.Empty
        };
    }

    private static ProductionCountry ConvertNodeToCountry(IEntity node)
    {
        return new ProductionCountry
        {
            Iso31661 = node.Properties.ContainsKey("iso_3166_1")
                ? node.Properties["iso_3166_1"].As<string>()
                : string.Empty,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : string.Empty
        };
    }

    private static SpokenLanguage ConvertNodeToLanguage(IEntity node)
    {
        return new SpokenLanguage
        {
            EnglishName = node.Properties.ContainsKey("englishName")
                ? node.Properties["englishName"].As<string>()
                : null,
            Iso6391 = node.Properties.ContainsKey("iso_639_1")
                ? node.Properties["iso_639_1"].As<string>()
                : null,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : null
        };
    }
}