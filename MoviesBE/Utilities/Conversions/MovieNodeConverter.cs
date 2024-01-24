using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class MovieNodeConverter
{
    public static Movie ConvertNodeToMovie(IEntity node)
    {
        return new Movie
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Title = node.Properties.ContainsKey("title") ? node.Properties["title"].As<string>() : string.Empty,
            ReleaseDate = node.Properties.ContainsKey("releaseDate")
                ? node.Properties["releaseDate"].As<string>()
                : string.Empty,
            Overview =
                node.Properties.ContainsKey("overview") ? node.Properties["overview"].As<string>() : string.Empty,
            Adult = node.Properties.ContainsKey("adult") ? node.Properties["adult"].As<bool>() : false,
            BackdropPath = node.Properties.ContainsKey("backdropPath")
                ? node.Properties["backdropPath"].As<string>()
                : string.Empty,
            Budget = node.Properties.ContainsKey("budget") ? node.Properties["budget"].As<long>() : 0L,
            Homepage =
                node.Properties.ContainsKey("homepage") ? node.Properties["homepage"].As<string>() : string.Empty,
            ImdbId = node.Properties.ContainsKey("imdbId") ? node.Properties["imdbId"].As<string>() : string.Empty,
            OriginalLanguage = node.Properties.ContainsKey("originalLanguage")
                ? node.Properties["originalLanguage"].As<string>()
                : string.Empty,
            OriginalTitle = node.Properties.ContainsKey("originalTitle")
                ? node.Properties["originalTitle"].As<string>()
                : string.Empty,
            Popularity = node.Properties.ContainsKey("popularity") ? node.Properties["popularity"].As<double>() : 0.0,
            PosterPath = node.Properties.ContainsKey("posterPath")
                ? node.Properties["posterPath"].As<string>()
                : string.Empty,
            Revenue = node.Properties.ContainsKey("revenue") ? node.Properties["revenue"].As<long>() : 0L,
            Runtime = node.Properties.ContainsKey("runtime") ? node.Properties["runtime"].As<int>() : 0,
            Status = node.Properties.ContainsKey("status") ? node.Properties["status"].As<string>() : string.Empty,
            Tagline = node.Properties.ContainsKey("tagline") ? node.Properties["tagline"].As<string>() : string.Empty,
            Video = node.Properties.ContainsKey("video") ? node.Properties["video"].As<bool>() : false,
            VoteAverage =
                node.Properties.ContainsKey("voteAverage") ? node.Properties["voteAverage"].As<double>() : 0.0,
            VoteCount = node.Properties.ContainsKey("voteCount") ? node.Properties["voteCount"].As<int>() : 0
            // Handle complex properties like Genres, ProductionCompanies, etc. here
        };
    }
}