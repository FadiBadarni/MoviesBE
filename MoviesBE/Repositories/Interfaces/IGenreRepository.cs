﻿using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Interfaces;

public interface IGenreRepository
{
    Task SaveGenresAsync(Movie movie, IAsyncQueryRunner tx);
}