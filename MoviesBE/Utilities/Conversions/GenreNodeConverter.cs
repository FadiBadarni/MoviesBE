using MoviesBE.Data;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class GenreNodeConverter
{
    public static Genre ConvertNodeToGenre(IEntity node)
    {
        return new Genre
        {
            Id = node.Properties.ContainsKey("id") ? node.Properties["id"].As<int>() : 0,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : string.Empty
        };
    }
}