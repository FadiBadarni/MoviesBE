namespace MoviesBE.Entities;

public class PaginationTracker
{
    public string Category { get; set; } //  "TMDB_TopRated", "IMDB_Popular"
    public int LastFetchedPage { get; set; }
}