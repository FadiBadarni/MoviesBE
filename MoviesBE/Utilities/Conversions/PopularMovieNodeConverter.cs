using MoviesBE.DTOs;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class PopularMovieNodeConverter
{
    public static PopularMovie ConvertNodeToPopularMovie(INode movieNode, IEnumerable<INode> genreNodes)
    {
        var popularMovie = new PopularMovie
        {
            Id = movieNode.Properties["id"].As<int>(),
            Title = movieNode.Properties["title"].As<string>(),
            PosterPath = movieNode.Properties["posterPath"].As<string>(),
            ReleaseDate = movieNode.Properties["releaseDate"].As<string>(),
            Overview = movieNode.Properties["overview"].As<string>(),
            Runtime = movieNode.Properties.ContainsKey("runtime") ? movieNode.Properties["runtime"].As<int>() : 0,
            Genres = genreNodes.Select(GenreNodeConverter.ConvertNodeToGenre).ToList()
        };
        return popularMovie;
    }
}