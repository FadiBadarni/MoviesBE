using MoviesBE.Data;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class PopularMovieNodeConverter
{
    public static PopularMovie ConvertNodeToPopularMovie(IEntity node)
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
}