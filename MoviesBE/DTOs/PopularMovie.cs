﻿namespace MoviesBE.DTOs;

public class PopularMovie
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? PosterPath { get; set; }
    public string? ReleaseDate { get; set; }
    public string? Overview { get; set; }
}