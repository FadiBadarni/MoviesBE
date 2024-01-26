using Neo4j.Driver;

namespace MoviesBE.Services.TMDB;

public class PopularityThresholdService
{
    public async Task<double> GetPopularityThreshold(IAsyncSession session, int percentile)
    {
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
              RETURN percentileCont(m.popularity, $percentile)",
                new { percentile = percentile / 100.0 });

            var record = await cursor.SingleAsync();
            return record[0].As<double>();
        });

        return result;
    }
}