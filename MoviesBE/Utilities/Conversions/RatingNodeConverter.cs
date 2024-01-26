using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class RatingNodeConverter
{
    public static Rating ConvertNodeToRating(INode node)
    {
        return new Rating
        {
            Provider = node.Properties["provider"].As<string>(),
            Score = node.Properties["score"].As<double>()
        };
    }
}