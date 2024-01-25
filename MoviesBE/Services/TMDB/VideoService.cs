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
            { "Teaser", 2 },
            { "Featurette", 3 },
            { "Behind the Scenes", 4 },
            { "Clip", 5 },
            { "Bloopers", 6 }
        };

        // Sort and select videos
        return videos
            .Where(v => v.Official && v.Site == "YouTube")
            .OrderByDescending(v => v.PublishedAt)
            .ThenBy(v =>
            {
                typePriority.TryGetValue(v.Type, out var priority);
                return priority;
            })
            .ThenByDescending(v => v.Size)
            .Take(10) // Limit to 10 videos
            .ToList();
    }
}