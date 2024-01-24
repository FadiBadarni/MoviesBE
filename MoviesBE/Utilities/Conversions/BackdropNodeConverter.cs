using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class BackdropNodeConverter
{
    public static MovieBackdrop ConvertNodeToBackdrop(IEntity node)
    {
        return new MovieBackdrop
        {
            FilePath = node.Properties.ContainsKey("file_path")
                ? node.Properties["file_path"].As<string>()
                : string.Empty,
            VoteAverage = node.Properties.ContainsKey("vote_average")
                ? node.Properties["vote_average"].As<double>()
                : 0.0
        };
    }
}