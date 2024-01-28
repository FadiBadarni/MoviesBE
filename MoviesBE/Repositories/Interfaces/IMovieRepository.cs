﻿using MoviesBE.DTOs;
using MoviesBE.Entities;

namespace MoviesBE.Repositories.Interfaces;

public interface IMovieRepository
{
    Task SaveMovieAsync(Movie movie);

    Task<Movie?> GetMovieByIdAsync(int movieId);

    Task<List<PopularMovie>> GetCachedPopularMoviesAsync();

    Task<List<TopRatedMovie>> GetCachedTopRatedMoviesAsync();

    Task<List<Movie>> GetMoviesWithoutIMDbRatingAsync();

    Task<List<Movie>> GetMoviesWithoutRTRatingAsync();
    Task<List<Movie>> GetAllMoviesAsync();
}