﻿using MoviesBE.Entities;
using MoviesBE.Repositories.Interfaces;
using MoviesBE.Utilities.Conversions;
using Neo4j.Driver;

namespace MoviesBE.Repositories.Implementations;

public class CreditsRepository : ICreditsRepository
{
    public async Task SaveCreditsAsync(Credits credits, IAsyncQueryRunner tx)
    {
        var movieId = credits.Id;

        if (credits.Cast != null)
        {
            await SaveCastAsync(credits.Cast, movieId, tx);
        }

        if (credits.Crew != null)
        {
            await SaveCrewAsync(credits.Crew, movieId, tx);
        }
    }

    public async Task<List<CastMember>> GetMovieCastAsync(IAsyncQueryRunner tx, int movieId)
    {
        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie {id: $id})-[:HAS_CAST]->(cast:Cast)
          RETURN COLLECT(DISTINCT cast) as castMembers",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["castMembers"].As<List<INode>>()
                .Select(CreditsNodeConverter.ConvertNodeToCastMember)
                .OrderByDescending(c => c.Popularity) // Sort by popularity
                .ToList();
        }

        return new List<CastMember>();
    }

    public async Task<List<CrewMember>> GetMovieCrewAsync(IAsyncQueryRunner tx, int movieId)
    {
        var cursor = await tx.RunAsync(
            @"MATCH (m:Movie {id: $id})-[:HAS_CREW]->(crew:Crew)
          RETURN COLLECT(DISTINCT crew) as crewMembers",
            new { id = movieId });

        if (await cursor.FetchAsync())
        {
            return cursor.Current["crewMembers"].As<List<INode>>()
                .Select(CreditsNodeConverter.ConvertNodeToCrewMember)
                .ToList();
        }

        return new List<CrewMember>();
    }

    private async Task SaveCastAsync(IEnumerable<CastMember> cast, int movieId, IAsyncQueryRunner tx)
    {
        // Detach old cast relationships
        var detachQuery = @"MATCH (m:Movie {id: $movieId})-[r:HAS_CAST]->(c:Cast)
                            DELETE r";
        await tx.RunAsync(detachQuery, new { movieId });

        foreach (var member in cast)
        {
            var query = @"MERGE (c:Cast {id: $id})
                        ON CREATE SET
                            c.adult = $adult,
                            c.gender = $gender,
                            c.knownForDepartment = $knownForDepartment,
                            c.name = $name,
                            c.popularity = $popularity,
                            c.profilePath = $profilePath,
                            c.castId = $castId
                        ON MATCH SET
                            c.adult = $adult,
                            c.gender = $gender,
                            c.knownForDepartment = $knownForDepartment,
                            c.name = $name,
                            c.popularity = $popularity,
                            c.profilePath = $profilePath,
                            c.castId = $castId
                        WITH c
                        MATCH (m:Movie {id: $movieId})
                        MERGE (m)-[:HAS_CAST {character: $character, creditId: $creditId, order: $order}]->(c)";

            await tx.RunAsync(query, new
            {
                id = member.Id,
                adult = member.Adult,
                gender = member.Gender,
                knownForDepartment = member.KnownForDepartment,
                name = member.Name,
                popularity = member.Popularity,
                profilePath = member.ProfilePath,
                castId = member.CastId,
                character = member.Character,
                creditId = member.CreditId,
                order = member.Order,
                movieId
            });
        }
    }

    private async Task SaveCrewAsync(IEnumerable<CrewMember> crew, int movieId, IAsyncQueryRunner tx)
    {
        // Detach old crew relationships
        var detachQuery = @"MATCH (m:Movie {id: $movieId})-[r:HAS_CREW]->(cr:Crew)
                            DELETE r";
        await tx.RunAsync(detachQuery, new { movieId });

        foreach (var member in crew)
        {
            var query = @"MERGE (cr:Crew {id: $id})
                            ON CREATE SET
                                cr.adult = $adult,
                                cr.gender = $gender,
                                cr.knownForDepartment = $knownForDepartment,
                                cr.name = $name,
                                cr.popularity = $popularity,
                                cr.profilePath = $profilePath,
                                cr.creditId = $creditId
                            ON MATCH SET
                                cr.adult = $adult,
                                cr.gender = $gender,
                                cr.knownForDepartment = $knownForDepartment,
                                cr.name = $name,
                                cr.popularity = $popularity,
                                cr.profilePath = $profilePath,
                                cr.creditId = $creditId
                            WITH cr
                            MATCH (m:Movie {id: $movieId})
                            MERGE (m)-[:HAS_CREW {department: $department, job: $job}]->(cr)";

            await tx.RunAsync(query, new
            {
                id = member.Id,
                adult = member.Adult,
                gender = member.Gender,
                knownForDepartment = member.KnownForDepartment,
                name = member.Name,
                popularity = member.Popularity,
                profilePath = member.ProfilePath,
                creditId = member.CreditId,
                department = member.Department,
                job = member.Job,
                movieId
            });
        }
    }
}