using Neo4j.Driver;

namespace MoviesBE.Services.TMDB;

public class RatingThresholdService
{
    public async Task<double> GetRatingThreshold(IAsyncSession session)
    {
        // Determine the minimum votes required for a movie to be considered in top rated list
        var minimumVotesThreshold = await CalculateMinimumVotesThreshold(session);

        // Get the global averages
        var (globalAvgVoteCount, globalAvgVoteAverage) = await GetGlobalAverages(session);

        // Calculate the Bayesian average
        var threshold = await CalculateBayesianAverageThreshold(session, minimumVotesThreshold, globalAvgVoteCount,
            globalAvgVoteAverage);

        return threshold;
    }

    private async Task<int> CalculateMinimumVotesThreshold(IAsyncSession session)
    {
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
                  RETURN percentileCont(m.voteCount, 0.8) AS voteCount80thPercentile");
            var record = await cursor.SingleAsync();
            return Convert.ToInt32(record["voteCount80thPercentile"].As<double>());
        });

        return result;
    }

    private async Task<(double avgVoteCount, double avgVoteAverage)> GetGlobalAverages(IAsyncSession session)
    {
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
                  RETURN AVG(m.voteCount) AS avgVoteCount, AVG(m.voteAverage) AS avgVoteAverage");
            var record = await cursor.SingleAsync();
            return (record["avgVoteCount"].As<double>(), record["avgVoteAverage"].As<double>());
        });

        return result;
    }

    private async Task<double> CalculateBayesianAverageThreshold(IAsyncSession session, int minimumVotesThreshold,
        double globalAvgVoteCount, double globalAvgVoteAverage)
    {
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                @"MATCH (m:Movie)
                  WHERE m.voteCount >= $minimumVotesThreshold
                  RETURN percentileCont((($globalAvgVoteCount * $globalAvgVoteAverage) + (m.voteCount * m.voteAverage)) / ($globalAvgVoteCount + m.voteCount), 0.9)",
                new { minimumVotesThreshold, globalAvgVoteCount, globalAvgVoteAverage });
            var record = await cursor.SingleAsync();
            return record[0].As<double>();
        });

        return result;
    }

    public async Task<(double ratingThreshold, int minimumVotesThreshold)> GetThresholds(IAsyncSession session)
    {
        // Calculate the minimum votes threshold
        var minimumVotesThreshold = await CalculateMinimumVotesThreshold(session);

        // Retrieve global average vote count and average vote rating
        var (globalAvgVoteCount, globalAvgVoteAverage) = await GetGlobalAverages(session);

        // Calculate the Bayesian average threshold using all required parameters
        var ratingThreshold = await CalculateBayesianAverageThreshold(session, minimumVotesThreshold,
            globalAvgVoteCount, globalAvgVoteAverage);

        return (ratingThreshold, minimumVotesThreshold);
    }
}