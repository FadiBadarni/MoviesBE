using MoviesBE.Data;
using Neo4j.Driver;

namespace MoviesBE.Services;

public class Neo4JService
{
    private readonly IDriver _neo4JDriver;

    public Neo4JService(IDriver neo4JDriver)
    {
        _neo4JDriver = neo4JDriver;
    }

    public async Task SaveMovieAsync(Movie movie)
    {
        var session = _neo4JDriver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync(
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
                  RETURN m",
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

                // Need to handle relationships
            });
        }
        finally
        {
            await session.CloseAsync();
        }
    }
}