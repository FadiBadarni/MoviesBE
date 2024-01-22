using Newtonsoft.Json;

namespace MoviesBE.Data;

public class Movie
{
    public bool Adult { get; set; }

    [JsonProperty("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonProperty("belongs_to_collection")]
    public MovieCollection? BelongsToCollection { get; set; }

    public long Budget { get; set; }

    public List<Genre>? Genres { get; set; }

    public string? Homepage { get; set; }

    public int Id { get; set; }

    [JsonProperty("imdb_id")]
    public string? ImdbId { get; set; }

    [JsonProperty("original_language")]
    public string? OriginalLanguage { get; set; }

    [JsonProperty("original_title")]
    public string? OriginalTitle { get; set; }

    public string? Overview { get; set; }

    public double Popularity { get; set; }

    [JsonProperty("poster_path")]
    public string? PosterPath { get; set; }

    [JsonProperty("production_companies")]
    public List<ProductionCompany>? ProductionCompanies { get; set; }

    [JsonProperty("production_countries")]
    public List<ProductionCountry>? ProductionCountries { get; set; }

    [JsonProperty("release_date")]
    public string? ReleaseDate { get; set; }

    public long Revenue { get; set; }

    public int Runtime { get; set; }

    [JsonProperty("spoken_languages")]
    public List<SpokenLanguage>? SpokenLanguages { get; set; }

    public string? Status { get; set; }

    public string? Tagline { get; set; }

    public string? Title { get; set; }

    public bool Video { get; set; }

    [JsonProperty("vote_average")]
    public double VoteAverage { get; set; }

    [JsonProperty("vote_count")]
    public int VoteCount { get; set; }

    public class Genre
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class MovieCollection
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        [JsonProperty("poster_path")]
        public string? PosterPath { get; set; }
        [JsonProperty("backdrop_path")]
        public string? BackdropPath { get; set; }
    }

    public class ProductionCompany
    {
        public int Id { get; set; }
        [JsonProperty("logo_path")]
        public string? LogoPath { get; set; }
        public string? Name { get; set; }
        [JsonProperty("origin_country")]
        public string? OriginCountry { get; set; }
    }

    public class ProductionCountry
    {
        [JsonProperty("iso_3166_1")]
        public string? Iso31661 { get; set; }
        public string? Name { get; set; }
    }

    public class SpokenLanguage
    {
        [JsonProperty("english_name")]
        public string? EnglishName { get; set; }
        [JsonProperty("iso_639_1")]
        public string? Iso6391 { get; set; }
        public string? Name { get; set; }
    }
}