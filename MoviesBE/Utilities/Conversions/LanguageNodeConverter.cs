using MoviesBE.Data;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class LanguageNodeConverter
{
    public static SpokenLanguage ConvertNodeToLanguage(IEntity node)
    {
        return new SpokenLanguage
        {
            EnglishName = node.Properties.ContainsKey("englishName")
                ? node.Properties["englishName"].As<string>()
                : null,
            Iso6391 = node.Properties.ContainsKey("iso_639_1")
                ? node.Properties["iso_639_1"].As<string>()
                : null,
            Name = node.Properties.ContainsKey("name") ? node.Properties["name"].As<string>() : null
        };
    }
}