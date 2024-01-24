using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class CompanyNodeConverter
{
    public static ProductionCompany ConvertNodeToCompany(IEntity node)
    {
        return new ProductionCompany
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : string.Empty,
            LogoPath =
                node.Properties.ContainsKey("logoPath") ? node.Properties["logoPath"].As<string>() : string.Empty,
            OriginCountry = node.Properties.ContainsKey("originCountry")
                ? node.Properties["originCountry"].As<string>()
                : string.Empty
        };
    }
}