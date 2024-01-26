using Neo4j.Driver;

namespace MoviesBE.Services.TMDB;

public class RatingThresholdService
{
    public async Task<double> GetRatingThreshold(IAsyncSession session)
    {
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
                  RETURN percentileCont(m.voteAverage, 0.9)");

            var record = await cursor.SingleAsync();
            return record[0].As<double>();
        });

        return result;
    }
}