using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class BackdropNodeConverter
{
    public static MovieBackdrop ConvertNodeToBackdrop(IEntity node)
    {
        return new MovieBackdrop
        {
            FilePath = node.Properties.ContainsKey("filePath")
                ? node.Properties["filePath"].As<string>()
                : string.Empty,
            VoteAverage = node.Properties.ContainsKey("voteAverage")
                ? node.Properties["voteAverage"].As<double>()
                : 0.0
        };
    }
}