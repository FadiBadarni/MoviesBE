using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class MovieVideoNodeConverter
{
    public static MovieVideo ConvertNodeToVideo(IEntity node)
    {
        return new MovieVideo
        {
            Id = node.Properties.TryGetValue("id", out var idValue) && idValue != null
                ? idValue.As<string>()
                : string.Empty,
            Iso6391 = node.Properties.TryGetValue("iso_639_1", out var iso6391Value) && iso6391Value != null
                ? iso6391Value.As<string>()
                : string.Empty,
            Iso31661 = node.Properties.TryGetValue("iso_3166_1", out var iso31661Value) && iso31661Value != null
                ? iso31661Value.As<string>()
                : string.Empty,
            Key = node.Properties.TryGetValue("key", out var keyValue) && keyValue != null
                ? keyValue.As<string>()
                : string.Empty,
            Name = node.Properties.TryGetValue("name", out var nameValue) && nameValue != null
                ? nameValue.As<string>()
                : string.Empty,
            Site = node.Properties.TryGetValue("site", out var siteValue) && siteValue != null
                ? siteValue.As<string>()
                : string.Empty,
            Size = node.Properties.TryGetValue("size", out var sizeValue) && sizeValue != null
                ? sizeValue.As<int>()
                : 0,
            Type = node.Properties.TryGetValue("type", out var typeValue) && typeValue != null
                ? typeValue.As<string>()
                : string.Empty,
            Official = node.Properties.TryGetValue("official", out var officialValue) && officialValue.As<bool>(),
            PublishedAt =
                node.Properties.TryGetValue("published_at", out var publishedAtValue) && publishedAtValue != null
                    ? publishedAtValue.As<DateTime>()
                    : default
        };
    }
}