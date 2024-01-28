using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
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
}