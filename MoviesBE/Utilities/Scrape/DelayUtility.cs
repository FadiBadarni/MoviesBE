using System.Security.Cryptography;

namespace MoviesBE.Utilities.Scrape;

public static class DelayUtility
{
    public static int GetRandomizedDelay(int baseDelay)
    {
        var jitterRange = GetJitterRange();
        using var rng = RandomNumberGenerator.Create();
        var jitterBytes = new byte[4];
        rng.GetBytes(jitterBytes);
        var jitter = BitConverter.ToInt32(jitterBytes, 0);

        // Ensure jitter is non-negative and within the jitter range
        jitter = Math.Abs(jitter % jitterRange);

        // Calculate the total delay, ensuring it's within the acceptable range
        var totalDelay = Math.Max(0, baseDelay + jitter - jitterRange / 2);

        // Ensure the delay is within the acceptable range for Task.Delay
        return Math.Min(totalDelay, int.MaxValue);
    }

    private static int GetJitterRange()
    {
        // Dynamically determine the jitter range
        var hour = DateTime.Now.Hour;
        return hour >= 8 && hour <= 18 ? 10000 : 5000; // Peak hours have a larger range
    }
}