using Movies.Application.DataTransferObjects;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public interface IUserWatchlistRepository
    {
        Task<bool> CreateAsync(UserWatchlist movie, CancellationToken token = default);
        Task<UserWatchlist?> GetByIdAsync(Guid id, bool isAdmin = false, Guid? userId = default, CancellationToken token = default);
        Task<IEnumerable<MovieDto>> GetAllAsync(bool isAdmin, string userId = null, CancellationToken token = default);
        Task<bool> UpdateAsync(UserWatchlist movie, CancellationToken token = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
        Task<bool> CountUserWatchlistAsync(string userId, CancellationToken token = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);
        Task<int> GetCountAsync(string? title, string? yearOfRelease, CancellationToken token = default);
        Task<UserWatchlist> AddMovieInWatchlistAsync(UserWatchlist userWatchlist, CancellationToken token = default);
    }
}
