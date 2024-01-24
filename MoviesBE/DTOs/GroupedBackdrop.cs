using MoviesBE.Entities;

namespace MoviesBE.DTOs;

public class GroupedBackdrop
{
    public double AspectRatio { get; set; }
    public long Resolution { get; set; }
    public List<MovieBackdrop> Backdrops { get; set; }
}