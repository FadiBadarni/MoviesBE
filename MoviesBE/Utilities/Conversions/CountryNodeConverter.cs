using MoviesBE.Data;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class CountryNodeConverter
{
    public static ProductionCountry ConvertNodeToCountry(IEntity node)
    {
        return new ProductionCountry
        {
            Iso31661 = node.Properties.ContainsKey("iso_3166_1")
                ? node.Properties["iso_3166_1"].As<string>()
                : string.Empty,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : string.Empty
        };
    }
}