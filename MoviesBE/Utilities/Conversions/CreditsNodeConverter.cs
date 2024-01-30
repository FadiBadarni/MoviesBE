using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class CreditsNodeConverter
{
    public static CastMember ConvertNodeToCastMember(IEntity node)
    {
        return new CastMember
        {
            Id = node.Properties.GetValueOrDefault("id", 0).As<int>(),
            Adult = node.Properties.GetValueOrDefault("adult", false).As<bool>(),
            Gender = node.Properties.GetValueOrDefault("gender", 0).As<int>(),
            KnownForDepartment = node.Properties.GetValueOrDefault("knownForDepartment", string.Empty).As<string>(),
            Name = node.Properties.GetValueOrDefault("name", string.Empty).As<string>(),
            Popularity = node.Properties.GetValueOrDefault("popularity", 0.0).As<double>(),
            ProfilePath = node.Properties.GetValueOrDefault("profilePath", null).As<string>(),
            CastId = node.Properties.GetValueOrDefault("castId", 0).As<int>()
        };
    }

    public static CrewMember ConvertNodeToCrewMember(IEntity node)
    {
        return new CrewMember
        {
            Id = node.Properties.GetValueOrDefault("id", 0).As<int>(),
            Adult = node.Properties.GetValueOrDefault("adult", false).As<bool>(),
            Gender = node.Properties.GetValueOrDefault("gender", 0).As<int>(),
            KnownForDepartment = node.Properties.GetValueOrDefault("knownForDepartment", string.Empty).As<string>(),
            Name = node.Properties.GetValueOrDefault("name", string.Empty).As<string>(),
            Popularity = node.Properties.GetValueOrDefault("popularity", 0.0).As<double>(),
            ProfilePath = node.Properties.GetValueOrDefault("profilePath", null).As<string>(),
            CreditId = node.Properties.GetValueOrDefault("creditId", string.Empty).As<string>(),
            Department = node.Properties.GetValueOrDefault("department", string.Empty).As<string>(),
            Job = node.Properties.GetValueOrDefault("job", string.Empty).As<string>()
        };
    }
}