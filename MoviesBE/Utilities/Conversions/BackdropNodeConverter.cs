using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class BackdropNodeConverter
{
    public static MovieBackdrop ConvertNodeToBackdrop(IEntity node)
    {
        return new MovieBackdrop
        {
            FilePath = node.Properties.TryGetValue("filePath", out var filePathValue)
                ? filePathValue.As<string>()
                : string.Empty,
            VoteAverage = node.Properties.TryGetValue("voteAverage", out var voteAverageValue)
                ? voteAverageValue.As<double>()
                : 0.0,
            AspectRatio = node.Properties.TryGetValue("aspectRatio", out var aspectRatioValue)
                ? aspectRatioValue.As<double>()
                : 0.0,
            Width = node.Properties.TryGetValue("width", out var widthValue) ? widthValue.As<int>() : 0,
            Height = node.Properties.TryGetValue("height", out var heightValue) ? heightValue.As<int>() : 0
        };
    }
}