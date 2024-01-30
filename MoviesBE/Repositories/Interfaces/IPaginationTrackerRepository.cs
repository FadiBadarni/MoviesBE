namespace MoviesBE.Repositories.Interfaces;

public interface IPaginationTrackerRepository
{
    Task UpdateLastFetchedPageAsync(string category, int lastFetchedPage);
    Task<int> GetLastFetchedPageAsync(string category);
}