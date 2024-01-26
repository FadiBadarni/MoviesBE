using MoviesBE.Entities;

namespace MoviesBE.Services.TMDB;

public class VideoService
{
    public List<MovieVideo> OrganizeMovieVideos(List<MovieVideo> videos)
    {
        // Define priorities for video types
        var typePriority = new Dictionary<string, int>
        {
            { "Trailer", 1 },
            { "Teaser", 2 } // Include Teasers as they are often similar to trailers
        };

        // Filter and sort videos
        return videos
            .Where(v => v.Official && v.Site == "YouTube" && typePriority.ContainsKey(v.Type))
            .OrderByDescending(v => v.PublishedAt)
            .ThenBy(v => typePriority[v.Type])
            .ThenByDescending(v => v.Size)
            .Take(5) // Limit to 5 videos
            .ToList();
    }
}