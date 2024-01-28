using MoviesBE.Entities;

namespace MoviesBE.DTOs;

public class TopRatedMovie : BaseMovie
{
    public List<Rating> Ratings { get; set; } = new();
}