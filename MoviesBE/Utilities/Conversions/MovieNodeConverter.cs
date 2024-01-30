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
            Adult = node.Properties.GetValueOrDefault("adult", false).As<bool>(),
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
            Video = node.Properties.GetValueOrDefault("video", false).As<bool>(),
            VoteAverage =
                node.Properties.ContainsKey("voteAverage") ? node.Properties["voteAverage"].As<double>() : 0.0,
            VoteCount = node.Properties.ContainsKey("voteCount") ? node.Properties["voteCount"].As<int>() : 0
        };
    }

    public static Movie ConvertNodeToMovieWithEssentialDetails(IEntity movieNode)
    {
        var movie = new Movie
        {
            Id = movieNode.Properties["id"].As<int>(),
            Title = movieNode.Properties["title"].As<string>(),
            Overview = movieNode.Properties["overview"].As<string>(),
            ReleaseDate = movieNode.Properties["releaseDate"].As<string>(),
            PosterPath = movieNode.Properties["posterPath"].As<string>(),
            Runtime = movieNode.Properties.GetValueOrDefault("runtime", 0).As<int>(),
            Status = movieNode.Properties.GetValueOrDefault("status", string.Empty).As<string>(),
            VoteAverage = movieNode.Properties.GetValueOrDefault("voteAverage", 0.0).As<double>(),

            BelongsToCollection = ExtractCollection(movieNode),
            Genres = ExtractGenres(movieNode),
            Backdrops = ExtractBackdrops(movieNode),
            Trailers = ExtractVideos(movieNode),
            Credits = ExtractCredits(movieNode),
            ProductionCompanies = ExtractCompanies(movieNode),
            ProductionCountries = ExtractCountries(movieNode),
            SpokenLanguages = ExtractLanguages(movieNode),
            Ratings = ExtractRatings(movieNode)
        };

        return movie;
    }

    private static List<MovieBackdrop> ExtractBackdrops(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("backdrops", out var backdropsValue) &&
            backdropsValue is List<IEntity> backdropNodes)
        {
            return backdropNodes.Select(BackdropNodeConverter.ConvertNodeToBackdrop).ToList();
        }

        return new List<MovieBackdrop>();
    }

    private static List<ProductionCompany> ExtractCompanies(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("companies", out var companiesValue) &&
            companiesValue is List<IEntity> companyNodes)
        {
            return companyNodes.Select(CompanyNodeConverter.ConvertNodeToCompany).ToList();
        }

        return new List<ProductionCompany>();
    }

    private static List<ProductionCountry> ExtractCountries(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("countries", out var countriesValue) &&
            countriesValue is List<IEntity> countryNodes)
        {
            return countryNodes.Select(CountryNodeConverter.ConvertNodeToCountry).ToList();
        }

        return new List<ProductionCountry>();
    }

    private static Credits ExtractCredits(IEntity movieNode)
    {
        var credits = new Credits();
        if (movieNode.Properties.TryGetValue("cast", out var castValue) && castValue is List<IEntity> castNodes)
        {
            credits.Cast = castNodes.Select(CreditsNodeConverter.ConvertNodeToCastMember).ToList();
        }

        if (movieNode.Properties.TryGetValue("crew", out var crewValue) && crewValue is List<IEntity> crewNodes)
        {
            credits.Crew = crewNodes.Select(CreditsNodeConverter.ConvertNodeToCrewMember).ToList();
        }

        return credits;
    }

    private static List<Genre> ExtractGenres(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("genres", out var genresValue) && genresValue is List<IEntity> genreNodes)
        {
            return genreNodes.Select(GenreNodeConverter.ConvertNodeToGenre).ToList();
        }

        return new List<Genre>();
    }

    private static List<SpokenLanguage> ExtractLanguages(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("languages", out var languagesValue) &&
            languagesValue is List<IEntity> languageNodes)
        {
            return languageNodes.Select(LanguageNodeConverter.ConvertNodeToLanguage).ToList();
        }

        return new List<SpokenLanguage>();
    }

    private static List<MovieVideo> ExtractVideos(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("videos", out var videosValue) && videosValue is List<IEntity> videoNodes)
        {
            return videoNodes.Select(MovieVideoNodeConverter.ConvertNodeToVideo).ToList();
        }

        return new List<MovieVideo>();
    }


    private static List<Rating> ExtractRatings(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("ratings", out var ratingsValue) &&
            ratingsValue is List<IEntity> ratingEntities)
        {
            return ratingEntities
                .Where(entity => entity is INode)
                .Select(entity => RatingNodeConverter.ConvertNodeToRating((INode)entity))
                .ToList();
        }

        return new List<Rating>();
    }

    private static MovieCollection ExtractCollection(IEntity movieNode)
    {
        if (movieNode.Properties.TryGetValue("collection", out var collectionValue) &&
            collectionValue is IEntity collectionNode)
        {
            return new MovieCollection
            {
                Id = collectionNode.Properties["id"].As<int>(),
                Name = collectionNode.Properties["name"].As<string>(),
                PosterPath = collectionNode.Properties["posterPath"].As<string>(),
                BackdropPath = collectionNode.Properties["backdropPath"].As<string>()
            };
        }

        return null;
    }
}