using System.Collections.Concurrent;

namespace WatchlistService.Infrastructure.Events;

public class MovieEventRegistry
{
    private readonly ConcurrentDictionary<int, DateTime> _deletedMovies = new();

    public void MarkAsDeleted(int movieId)
    {
        _deletedMovies[movieId] = DateTime.UtcNow;
    }

    public bool WasRecentlyDeleted(int movieId, TimeSpan? window = null)
    {
        if (_deletedMovies.TryGetValue(movieId, out var deletedAt))
        {
            if (window == null || (DateTime.UtcNow - deletedAt) <= window)
                return true;
        }

        return false;
    }
}