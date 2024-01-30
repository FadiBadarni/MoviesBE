using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class MVideoRepository : IMVideoRepository
{
    public async Task SaveMovieVideosAsync(Movie movie, IAsyncQueryRunner tx)
    {
        if (movie.Trailers == null)
        {
            return;
        }

        // First, detach all existing video relationships from this movie to avoid duplicates
        await tx.RunAsync(
            @"MATCH (m:Movie {id: $movieId})-[r:HAS_VIDEO]->(v:Video)
          DELETE r",
            new { movieId = movie.Id });

        // For each video, merge the video node and create a relationship with the movie
        foreach (var video in movie.Trailers)
            await tx.RunAsync(
                @"MERGE (v:Video {id: $id})
              ON CREATE SET
                v.name = $name,
                v.key = $key,
                v.site = $site,
                v.type = $type,
                v.size = $size
              WITH v
              MATCH (m:Movie {id: $movieId})
              MERGE (m)-[:HAS_VIDEO]->(v)",
                new
                {
                    id = video.Id,
                    name = video.Name,
                    key = video.Key,
                    site = video.Site,
                    type = video.Type,
                    size = video.Size,
                    movieId = movie.Id
                });
    }

    public async Task<List<MovieVideo>> GetMovieVideosAsync(IAsyncQueryRunner tx, int movieId)
    {
        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie)-[:HAS_VIDEO]->(v:Video) WHERE m.id = $id RETURN COLLECT(DISTINCT v) as videos",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["videos"].As<List<INode>>()
                .Select(MovieVideoNodeConverter.ConvertNodeToVideo).ToList();
        }

        return new List<MovieVideo>();
    }
}