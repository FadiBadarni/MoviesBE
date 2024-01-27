using MoviesBE.DTOs;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class TopRatedMovieNodeConverter
{
    public static TopRatedMovie ConvertNodeToTopRatedMovie(IEntity movieNode, IEnumerable<INode> ratingNodes)
    {
        var topRatedMovie = new TopRatedMovie
        {
            Id = movieNode.Properties.ContainsKey("id") ? movieNode.Properties["id"].As<int>() : 0,
            Title = movieNode.Properties.ContainsKey("title")
                ? movieNode.Properties["title"].As<string>()
                : string.Empty,
            PosterPath = movieNode.Properties.ContainsKey("posterPath")
                ? movieNode.Properties["posterPath"].As<string>()
                : string.Empty,
            ReleaseDate = movieNode.Properties.ContainsKey("releaseDate")
                ? movieNode.Properties["releaseDate"].As<string>()
                : string.Empty,
            Overview = movieNode.Properties.ContainsKey("overview")
                ? movieNode.Properties["overview"].As<string>()
                : string.Empty
        };

        topRatedMovie.Ratings = ratingNodes.Select(RatingNodeConverter.ConvertNodeToRating).ToList();

        return topRatedMovie;
    }
}