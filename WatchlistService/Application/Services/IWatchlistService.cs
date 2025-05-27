using WatchlistService.Application.Dtos;

namespace WatchlistService.Application.Services;

public interface IWatchlistService
{
    /// <summary>
    /// Adds a movie to the user's watchlist.
    /// 
    /// Informal: Adds the specified movie if it's not already in the list.
    ///
    /// Semi-formal
    /// Pre-condition:
    /// - userId is not null or empty
    /// - dto.MovieId > 0
    /// - dto.MovieId not already in watchlist
    /// 
    /// Post-condition:
    /// - dto.MovieId is added to the user's watchlist
    /// - Watchlist contains no duplicate MovieIds
    ///
    /// Formal Specification (Mathematical logic):
    /// 
    /// Pre:
    ///   userId ≠ null ∧ userId ≠ "" ∧ dto.MovieId > 0 ∧
    ///   ∀m ∈ Watchlist(userId): m.MovieId ≠ dto.MovieId
    ///
    /// Post:
    ///   ∃m ∈ Watchlist(userId): m.MovieId = dto.MovieId ∧
    ///   ∀i, j ∈ [0, n), i ≠ j ⇒ Watchlist(userId)[i].MovieId ≠ Watchlist(userId)[j].MovieId
    ///
    /// Notes:
    /// - Watchlist(userId) refers to the current list of movies for the given user
    /// - n = length of Watchlist(userId) after insertion
    /// </summary>
    Task AddMovieAsync(string userId, AddMovieDto dto);

    /// <summary>
    /// Removes a movie from the user's watchlist.
    /// 
    /// Informal: Deletes the movie from the list if it exists.
    /// 
    /// Pre-condition:
    /// - userId is not null or empty
    /// - movieId > 0
    /// 
    /// Post-condition:
    /// - movieId is removed from the watchlist if present
    /// </summary>
    Task RemoveMovieAsync(string userId, int movieId);

    /// <summary>
    /// Retrieves the full watchlist for the specified user.
    /// 
    /// Informal: Returns all movies added by the user.
    /// 
    /// Pre-condition:
    /// - userId is not null or empty
    /// 
    /// Post-condition:
    /// - Returns a valid WatchlistDto object
    /// </summary>
    Task<WatchlistDto> GetWatchlistAsync(string userId);

    /// <summary>
    /// Removes a movie from all users' watchlists.
    /// 
    /// Informal: If a movie is deleted from the platform, this removes it from everyone's list.
    /// 
    /// Pre-condition:
    /// - movieId > 0
    /// 
    /// Post-condition:
    /// - movieId is removed from all watchlists where it appears
    /// </summary>
    Task RemoveMovieFromAllWatchlistsAsync(int movieId);
}
