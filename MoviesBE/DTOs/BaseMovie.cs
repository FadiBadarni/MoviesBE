using MoviesBE.Entities;

namespace MoviesBE.DTOs;

public class BaseMovie
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? PosterPath { get; set; }
    public string? ReleaseDate { get; set; }
    public string? Overview { get; set; }
    public int Runtime { get; set; }
    public List<Genre>? Genres { get; set; }
}