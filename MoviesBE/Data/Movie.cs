using System.Text.Json.Serialization;

namespace MoviesBE.Data;

public class Movie
{
    public bool Adult { get; init; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; init; }

    [JsonPropertyName("belongs_to_collection")]
    public MovieCollection? BelongsToCollection { get; init; }

    public long Budget { get; init; }

    public List<Genre>? Genres { get; set; }

    [JsonPropertyName("genre_ids")]
    public List<int>? GenreIds { get; init; }

    public string? Homepage { get; init; }

    public int Id { get; init; }

    [JsonPropertyName("imdb_id")]
    public string? ImdbId { get; init; }

    [JsonPropertyName("original_language")]
    public string? OriginalLanguage { get; init; }

    [JsonPropertyName("original_title")]
    public string? OriginalTitle { get; init; }

    public string? Overview { get; init; }

    public double Popularity { get; init; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; init; }

    [JsonPropertyName("production_companies")]
    public List<ProductionCompany>? ProductionCompanies { get; set; }

    [JsonPropertyName("production_countries")]
    public List<ProductionCountry>? ProductionCountries { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; init; }

    public long Revenue { get; init; }

    public int Runtime { get; init; }

    [JsonPropertyName("spoken_languages")]
    public List<SpokenLanguage>? SpokenLanguages { get; init; }

    public string? Status { get; init; }

    public string? Tagline { get; init; }

    public string? Title { get; init; }

    public bool Video { get; init; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; init; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; init; }

    public List<MovieBackdrop>? Backdrops { get; set; }

    public class Genre
    {
        public int Id { get; init; }
        public string? Name { get; init; }
    }

    public class MovieCollection
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; init; }
        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; init; }
    }

    public class ProductionCompany
    {
        public int Id { get; init; }
        [JsonPropertyName("logo_path")]
        public string? LogoPath { get; init; }
        public string? Name { get; init; }
        [JsonPropertyName("origin_country")]
        public string? OriginCountry { get; init; }
    }

    public class ProductionCountry
    {
        [JsonPropertyName("iso_3166_1")]
        public string? Iso31661 { get; init; }
        public string? Name { get; init; }
    }

    public class SpokenLanguage
    {
        [JsonPropertyName("english_name")]
        public string? EnglishName { get; init; }
        [JsonPropertyName("iso_639_1")]
        public string? Iso6391 { get; init; }
        public string? Name { get; init; }
    }
}