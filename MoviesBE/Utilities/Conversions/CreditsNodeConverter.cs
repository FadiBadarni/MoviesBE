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
            KnownForDepartment = node.Properties.GetValueOrDefault("known_for_department", string.Empty).As<string>(),
            Name = node.Properties.GetValueOrDefault("name", string.Empty).As<string>(),
            Popularity = node.Properties.GetValueOrDefault("popularity", 0.0).As<double>(),
            ProfilePath = node.Properties.GetValueOrDefault("profile_path", null).As<string>(),
            CastId = node.Properties.GetValueOrDefault("cast_id", 0).As<int>(),
            Character = node.Properties.GetValueOrDefault("character", string.Empty).As<string>(),
            CreditId = node.Properties.GetValueOrDefault("credit_id", string.Empty).As<string>(),
            Order = node.Properties.GetValueOrDefault("order", 0).As<int>()
        };
    }

    public static CrewMember ConvertNodeToCrewMember(IEntity node)
    {
        return new CrewMember
        {
            Id = node.Properties.GetValueOrDefault("id", 0).As<int>(),
            Adult = node.Properties.GetValueOrDefault("adult", false).As<bool>(),
            Gender = node.Properties.GetValueOrDefault("gender", 0).As<int>(),
            KnownForDepartment = node.Properties.GetValueOrDefault("known_for_department", string.Empty).As<string>(),
            Name = node.Properties.GetValueOrDefault("name", string.Empty).As<string>(),
            OriginalName = node.Properties.GetValueOrDefault("original_name", string.Empty).As<string>(),
            Popularity = node.Properties.GetValueOrDefault("popularity", 0.0).As<double>(),
            ProfilePath = node.Properties.GetValueOrDefault("profile_path", null).As<string>(),
            CreditId = node.Properties.GetValueOrDefault("credit_id", string.Empty).As<string>(),
            Department = node.Properties.GetValueOrDefault("department", string.Empty).As<string>(),
            Job = node.Properties.GetValueOrDefault("job", string.Empty).As<string>()
        };
    }
}