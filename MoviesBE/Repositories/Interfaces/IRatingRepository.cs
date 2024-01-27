﻿using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IRatingRepository
{
    Task UpdateMovieRatingsAsync(int movieId, List<Rating> ratings);

    Task SaveMovieRatingsAsync(int movieId, List<Rating> ratings, IAsyncQueryRunner tx);
}