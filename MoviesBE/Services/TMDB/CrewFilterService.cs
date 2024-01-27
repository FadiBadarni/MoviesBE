using MoviesBE.Entities;

namespace MoviesBE.Services.TMDB;

public class CrewFilterService
{
    public Credits ProcessCredits(Credits credits)
    {
        credits.Cast = credits.Cast;
        if (credits.Crew != null)
        {
            credits.Crew = FilterKeyCrewMembers(credits.Crew);
        }

        return credits;
    }

    private static List<CrewMember> FilterKeyCrewMembers(IEnumerable<CrewMember> crew)
    {
        var keyRoles = new HashSet<string> { "Director", "Writer", "Producer" };

        // Filter the crew members to include only those with the specified key roles
        return crew.Where(member => member.Job != null && keyRoles.Contains(member.Job)).ToList();
    }
}