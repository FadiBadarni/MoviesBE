using MoviesBE.DTOs;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class TopRatedMovieNodeConverter
{
    public static TopRatedMovie ConvertNodeToTopRatedMovie(INode movieNode, IEnumerable<INode> ratingNodes,
        IEnumerable<INode> genreNodes)
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
                : string.Empty,
            Runtime = movieNode.Properties.ContainsKey("runtime") ? movieNode.Properties["runtime"].As<int>() : 0,
            Ratings = ratingNodes.Select(RatingNodeConverter.ConvertNodeToRating).ToList(),
            Genres = genreNodes.Select(GenreNodeConverter.ConvertNodeToGenre).ToList()
        };
        return topRatedMovie;
    }
}