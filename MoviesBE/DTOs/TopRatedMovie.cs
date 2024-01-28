using MoviesBE.Entities;

namespace MoviesBE.DTOs;

public class TopRatedMovie : BaseMovie
{
    public List<Rating> Ratings { get; set; } = new();
    public int Runtime { get; set; }
    public List<Genre>? Genres { get; set; }
}