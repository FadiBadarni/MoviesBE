using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class MBackdropRepository : IMBackdropRepository
{
    public async Task SaveMovieBackdropsAsync(Movie movie, IAsyncQueryRunner tx)
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

    public async Task<List<MovieBackdrop>> GetMovieBackdropsAsync(IAsyncQueryRunner tx, int movieId)
    {
        const double bannerAspectRatio = 1.78;

        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie)-[:HAS_BACKDROP]->(b:Backdrop) WHERE m.id = $id RETURN COLLECT(DISTINCT b) as backdrops",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["backdrops"].As<List<INode>>()
                .Select(BackdropNodeConverter.ConvertNodeToBackdrop)
                .OrderByDescending(b =>
                    Math.Abs(b.AspectRatio - bannerAspectRatio)) // Prioritize banner-like aspect ratio
                .ThenByDescending(b => b.Width * b.Height) // Then by size
                .ToList();
        }

        return new List<MovieBackdrop>();
    }
}