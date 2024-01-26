using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Repositories;

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

    private async Task SaveCastAsync(IEnumerable<CastMember> cast, int movieId, IAsyncQueryRunner tx)
    {
        // Detach old cast relationships
        var detachQuery = @"
        MATCH (m:Movie {id: $movieId})-[r:HAS_CAST]->(c:Cast)
        DELETE r";
        await tx.RunAsync(detachQuery, new { movieId });

        foreach (var member in cast)
        {
            var query = @"
            MERGE (c:Cast {id: $id})
            ON CREATE SET
                c.adult = $adult,
                c.gender = $gender,
                c.knownForDepartment = $knownForDepartment,
                c.name = $name,
                c.originalName = $originalName,
                c.popularity = $popularity,
                c.profilePath = $profilePath,
                c.castId = $castId,
                c.character = $character,
                c.creditId = $creditId,
                c.order = $order
            ON MATCH SET
                c.adult = $adult,
                c.gender = $gender,
                c.knownForDepartment = $knownForDepartment,
                c.name = $name,
                c.originalName = $originalName,
                c.popularity = $popularity,
                c.profilePath = $profilePath,
                c.castId = $castId,
                c.character = $character,
                c.creditId = $creditId,
                c.order = $order
            WITH c
            MATCH (m:Movie {id: $movieId})
            MERGE (m)-[:HAS_CAST]->(c)";

            await tx.RunAsync(query, new
            {
                id = member.Id,
                adult = member.Adult,
                gender = member.Gender,
                knownForDepartment = member.KnownForDepartment,
                name = member.Name,
                originalName = member.OriginalName,
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
        var detachQuery = @"
                            MATCH (m:Movie {id: $movieId})-[r:HAS_CREW]->(cr:Crew)
                            DELETE r";
        await tx.RunAsync(detachQuery, new { movieId });

        foreach (var member in crew)
        {
            var query = @"
            MERGE (cr:Crew {id: $id})
            ON CREATE SET
                cr.adult = $adult,
                cr.gender = $gender,
                cr.knownForDepartment = $knownForDepartment,
                cr.name = $name,
                cr.originalName = $originalName,
                cr.popularity = $popularity,
                cr.profilePath = $profilePath,
                cr.creditId = $creditId,
                cr.department = $department,
                cr.job = $job
            ON MATCH SET
                cr.adult = $adult,
                cr.gender = $gender,
                cr.knownForDepartment = $knownForDepartment,
                cr.name = $name,
                cr.originalName = $originalName,
                cr.popularity = $popularity,
                cr.profilePath = $profilePath,
                cr.creditId = $creditId,
                cr.department = $department,
                cr.job = $job
            WITH cr
            MATCH (m:Movie {id: $movieId})
            MERGE (m)-[:HAS_CREW]->(cr)";

            await tx.RunAsync(query, new
            {
                id = member.Id,
                adult = member.Adult,
                gender = member.Gender,
                knownForDepartment = member.KnownForDepartment,
                name = member.Name,
                originalName = member.OriginalName,
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