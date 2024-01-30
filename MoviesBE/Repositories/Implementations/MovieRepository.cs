using MoviesBE.DTOs;
using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Services.TMDB;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class MovieRepository : IMovieRepository
{
    private readonly IMBackdropRepository _backdropRepository;
    private readonly ICreditsRepository _creditsRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<MovieRepository> _logger;
    private readonly IMLanguageRepository _mLanguageRepository;
    private readonly IDriver _neo4JDriver;
    private readonly IPCompanyRepository _pCompanyRepository;
    private readonly IPCountryRepository _pCountryRepository;
    private readonly PopularityThresholdService _popularityThresholdService;
    private readonly IRatingRepository _ratingRepository;
    private readonly RatingThresholdService _ratingThresholdService;
    private readonly IMVideoRepository _videoRepository;

    public MovieRepository(IDriver neo4JDriver, ICreditsRepository creditsRepository,
        RatingThresholdService ratingThresholdService, PopularityThresholdService popularityThresholdService,
        ILogger<MovieRepository> logger, IGenreRepository genreRepository, IPCompanyRepository pCompanyRepository,
        IPCountryRepository pCountryRepository, IMLanguageRepository mLanguageRepository,
        IMBackdropRepository backdropRepository, IMVideoRepository videoRepository, IRatingRepository ratingRepository)
    {
        _neo4JDriver = neo4JDriver;
        _creditsRepository = creditsRepository;
        _ratingThresholdService = ratingThresholdService;
        _popularityThresholdService = popularityThresholdService;
        _logger = logger;
        _genreRepository = genreRepository;
        _pCompanyRepository = pCompanyRepository;
        _pCountryRepository = pCountryRepository;
        _mLanguageRepository = mLanguageRepository;
        _backdropRepository = backdropRepository;
        _videoRepository = videoRepository;
        _ratingRepository = ratingRepository;
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
                    await _genreRepository.SaveGenresAsync(movie, tx);
                }

                if (movie.ProductionCompanies != null)
                {
                    await _pCompanyRepository.SaveProductionCompaniesAsync(movie, tx);
                }

                if (movie.ProductionCountries != null)
                {
                    await _pCountryRepository.SaveProductionCountriesAsync(movie, tx);
                }

                if (movie.SpokenLanguages != null)
                {
                    await _mLanguageRepository.SaveSpokenLanguagesAsync(movie, tx);
                }

                if (movie.Backdrops != null)
                {
                    await _backdropRepository.SaveMovieBackdropsAsync(movie, tx);
                }

                if (movie.Trailers != null)
                {
                    await _videoRepository.SaveMovieVideosAsync(movie, tx);
                }

                if (movie.Credits != null)
                {
                    await _creditsRepository.SaveCreditsAsync(movie.Credits, tx);
                }
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
                        RETURN m",
                new { id = movieId });

            if (await cursor.FetchAsync())
            {
                var movieNode = cursor.Current["m"].As<INode>();
                var genres = await _genreRepository.GetMovieGenresAsync(tx, movieId);
                var companies = await _pCompanyRepository.GetMovieProductionCompaniesAsync(tx, movieId);
                var countries = await _pCountryRepository.GetMovieProductionCountriesAsync(tx, movieId);
                var languages = await _mLanguageRepository.GetMovieSpokenLanguagesAsync(tx, movieId);
                var backdrops = await _backdropRepository.GetMovieBackdropsAsync(tx, movieId);
                var videos = await _videoRepository.GetMovieVideosAsync(tx, movieId);
                var ratings = await _ratingRepository.GetMovieRatingsAsync(tx, movieId);

                var movie = MovieNodeConverter.ConvertNodeToMovie(movieNode);
                movie.Genres = genres;
                movie.ProductionCompanies = companies;
                movie.ProductionCountries = countries;
                movie.SpokenLanguages = languages;
                movie.Backdrops = backdrops;
                movie.Trailers = videos;
                movie.Credits = new Credits
                {
                    Id = movieId,
                    Cast = await _creditsRepository.GetMovieCastAsync(tx, movieId),
                    Crew = await _creditsRepository.GetMovieCrewAsync(tx, movieId)
                };
                movie.Ratings = ratings;

                return movie;
            }

            return null;
        });
    }


    public async Task<List<PopularMovie>> GetLimitedPopularMoviesAsync(int limit = 3)
    {
        return await FetchPopularMoviesAsync(limit);
    }

    public async Task<List<TopRatedMovie>> GetLimitedTopRatedMoviesAsync(int limit = 3)
    {
        return await FetchTopRatedMoviesAsync(limit);
    }


    public async Task<(List<PopularMovie>, int)> GetPopularMoviesAsync(int page, int pageSize)
    {
        var movies = new List<PopularMovie>();
        await using var session = _neo4JDriver.AsyncSession();

        var skip = (page - 1) * pageSize;

        var query = @"MATCH (m:Movie)
                        OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                        RETURN m, COLLECT(g) AS Genres
                        ORDER BY m.popularity DESC
                        SKIP $skip
                        LIMIT $pageSize";

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { skip, pageSize });
            return await cursor.ToListAsync();
        });

        foreach (var record in result)
        {
            var movieNode = record["m"].As<INode>();
            var genreNodes = record["Genres"].As<List<INode>>();
            var movie = PopularMovieNodeConverter.ConvertNodeToPopularMovie(movieNode, genreNodes);
            movies.Add(movie);
        }

        // Query for total count
        var countQuery = @"MATCH (m:Movie) RETURN count(m) as totalCount";
        var totalCount = 0;
        await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(countQuery);
            if (await cursor.FetchAsync())
            {
                totalCount = cursor.Current["totalCount"].As<int>();
            }
        });

        return (movies, totalCount);
    }

    public async Task<List<Movie>> GetMoviesWithoutIMDbRatingAsync()
    {
        var moviesToUpdate = new List<Movie>();
        await using var session = _neo4JDriver.AsyncSession();

        await session.ExecuteReadAsync(async tx =>
        {
            _logger.LogInformation("Fetching movies without IMDb ratings.");
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
                      WHERE m.imdbId IS NOT NULL
                      OPTIONAL MATCH (m)-[r:HAS_RATING]->(rating:Rating {provider: 'IMDb'})
                      WITH m, rating
                      WHERE rating IS NULL OR rating.score = 0
                      RETURN m");

            while (await cursor.FetchAsync())
            {
                var movieNode = cursor.Current["m"].As<INode>();
                var movie = MovieNodeConverter.ConvertNodeToMovie(movieNode);
                moviesToUpdate.Add(movie);
            }

            return moviesToUpdate;
        });

        _logger.LogInformation($"Total movies fetched for update: {moviesToUpdate.Count}");

        await session.CloseAsync();

        return moviesToUpdate;
    }


    public async Task<List<Movie>> GetMoviesWithoutRTRatingAsync()
    {
        var moviesToUpdate = new List<Movie>();
        await using var session = _neo4JDriver.AsyncSession();

        await session.ExecuteReadAsync(async tx =>
        {
            _logger.LogInformation("Fetching movies without Rotten Tomatoes ratings.");
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
                  WHERE m.title IS NOT NULL
                  OPTIONAL MATCH (m)-[r:HAS_RATING]->(rating:Rating {provider: 'RottenTomatoes'})
                  WITH m, rating
                  WHERE rating IS NULL OR rating.score = 0
                  RETURN m");

            while (await cursor.FetchAsync())
            {
                var movieNode = cursor.Current["m"].As<INode>();
                var movie = MovieNodeConverter.ConvertNodeToMovie(movieNode);
                moviesToUpdate.Add(movie);
            }

            return moviesToUpdate;
        });

        _logger.LogInformation($"Total movies fetched for update: {moviesToUpdate.Count}");

        await session.CloseAsync();

        return moviesToUpdate;
    }

    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        var movies = new List<Movie>();
        await using var session = _neo4JDriver.AsyncSession();

        await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
              OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
              OPTIONAL MATCH (m)-[:HAS_BACKDROP]->(b:Backdrop)
              OPTIONAL MATCH (m)-[:HAS_VIDEO]->(v:Video)
              OPTIONAL MATCH (m)-[:HAS_CAST]->(cast:Cast)
              OPTIONAL MATCH (m)-[:HAS_CREW]->(crew:Crew)
              RETURN m, 
                     COLLECT(DISTINCT g) as genres, 
                     COLLECT(DISTINCT b) as backdrops, 
                     COLLECT(DISTINCT v) as videos,
                     COLLECT(DISTINCT cast) as castMembers, 
                     COLLECT(DISTINCT crew) as crewMembers");

            while (await cursor.FetchAsync())
            {
                var movieNode = cursor.Current["m"].As<INode>();
                var movie = MovieNodeConverter.ConvertNodeToMovieWithEssentialDetails(movieNode);
                movies.Add(movie);
            }
        });

        return movies;
    }

    public async Task<(List<TopRatedMovie>, int)> GetTopRatedMoviesAsync(int page, int pageSize, string ratingFilter,
        int? genreFilter)
    {
        var movies = new List<TopRatedMovie>();
        await using var session = _neo4JDriver.AsyncSession();

        var skip = (page - 1) * pageSize;

        // Dynamic WHERE clause based on genreFilter
        var whereClause = genreFilter.HasValue ? "AND g.id = $genreFilter" : "";

        // Dynamic ORDER BY clause based on filterType
        var orderByClause = ratingFilter switch
        {
            "IMDb" => "ORDER BY COALESCE(maxIMDbRating, 0) DESC, COALESCE(maxRTRating, 0) DESC, m.voteCount DESC",
            "RottenTomatoes" =>
                "ORDER BY COALESCE(maxRTRating, 0) DESC, COALESCE(maxIMDbRating, 0) DESC, m.voteCount DESC",
            "TMDB" => "ORDER BY m.voteAverage DESC, m.voteCount DESC",
            _ => "ORDER BY m.voteAverage DESC, m.voteCount DESC" // Default sorting
        };

        var query = $@"MATCH (m:Movie)-[:HAS_GENRE]->(g:Genre)
                        OPTIONAL MATCH (m)-[rel:HAS_RATING]->(r:Rating)
                        WITH m, g, r
                        WHERE 1=1 {whereClause}
                        WITH m, 
                             COLLECT(DISTINCT r) AS Ratings, 
                             COLLECT(DISTINCT g) AS Genres,
                             MAX(CASE WHEN r.provider = 'IMDb' THEN r.score ELSE NULL END) AS maxIMDbRating,
                             MAX(CASE WHEN r.provider = 'RottenTomatoes' THEN r.score ELSE NULL END) AS maxRTRating
                        {orderByClause}
                        SKIP $skip
                        LIMIT $pageSize
                        RETURN m, Ratings, Genres";

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { skip, pageSize, genreFilter });
            return await cursor.ToListAsync();
        });

        foreach (var record in result)
        {
            var movieNode = record["m"].As<INode>();
            var ratingNodes = record["Ratings"].As<List<INode>>();
            var genreNodes = record["Genres"].As<List<INode>>();

            var movie = TopRatedMovieNodeConverter.ConvertNodeToTopRatedMovie(movieNode, ratingNodes, genreNodes);
            movies.Add(movie);
        }

        // Query for total count
        var countQuery = $@"MATCH (m:Movie)-[:HAS_GENRE]->(g:Genre)
                    WHERE 1=1 {whereClause}
                    RETURN count(DISTINCT m) as totalCount";
        var totalCount = 0;
        var queryParameters = new Dictionary<string, object>();
        if (genreFilter.HasValue)
        {
            queryParameters.Add("genreFilter", genreFilter.Value);
        }

        await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(countQuery, queryParameters);
            if (await cursor.FetchAsync())
            {
                totalCount = cursor.Current["totalCount"].As<int>();
            }
        });

        return (movies, totalCount);
    }


    private async Task<List<TopRatedMovie>> FetchTopRatedMoviesAsync(int? limit = null)
    {
        var movies = new List<TopRatedMovie>();
        await using var session = _neo4JDriver.AsyncSession();

        var (ratingThreshold, minimumVotesThreshold) = await _ratingThresholdService.GetThresholds(session);

        var query = @"MATCH (m:Movie)
                      WHERE m.voteCount >= $minimumVotesThreshold
                      OPTIONAL MATCH (m)-[rel:HAS_RATING]->(r:Rating)
                      OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                      WITH m, r, COLLECT(g) AS Genres
                      WHERE r IS NULL OR 
                            (r.provider = 'IMDb' AND r.score >= $ratingThreshold) OR 
                            (r.provider = 'RottenTomatoes' AND r.score >= $ratingThreshold)
                      RETURN m, COLLECT(r) AS Ratings, Genres";

        if (limit.HasValue)
        {
            query += " ORDER BY m.voteAverage DESC, m.voteCount DESC LIMIT $limit";
        }

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { ratingThreshold, minimumVotesThreshold, limit });
            return await cursor.ToListAsync();
        });

        // Populate movies list from the query result
        foreach (var record in result)
        {
            var movieNode = record["m"].As<INode>();
            var ratingNodes = record["Ratings"].As<List<INode>>();
            var genreNodes = record["Genres"].As<List<INode>>();

            var movie = TopRatedMovieNodeConverter.ConvertNodeToTopRatedMovie(movieNode, ratingNodes, genreNodes);
            movies.Add(movie);
        }

        return movies;
    }

    private async Task<List<PopularMovie>> FetchPopularMoviesAsync(int? limit = null)
    {
        var movies = new List<PopularMovie>();
        await using var session = _neo4JDriver.AsyncSession();

        var percentile = 90;
        var popularityThreshold = await _popularityThresholdService.GetPopularityThreshold(session, percentile);

        var query = @"MATCH (m:Movie)
                      WHERE m.popularity >= $popularityThreshold
                      OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                      RETURN m, COLLECT(g) AS Genres";

        if (limit.HasValue)
        {
            query += " ORDER BY m.popularity DESC LIMIT $limit";
        }

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { popularityThreshold, limit });
            return await cursor.ToListAsync();
        });

        // Populate movies list from the query result
        foreach (var record in result)
        {
            var movieNode = record["m"].As<INode>();
            var genreNodes = record["Genres"].As<List<INode>>();
            var movie = PopularMovieNodeConverter.ConvertNodeToPopularMovie(movieNode, genreNodes);
            movies.Add(movie);
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
}