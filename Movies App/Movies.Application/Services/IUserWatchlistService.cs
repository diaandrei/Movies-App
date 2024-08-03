using Movies.Application.DataTransferObjects;
using Movies.Application.Models;

namespace Movies.Application.Services
{
    public interface IUserWatchlistService
    {
        Task<bool> CreateAsync(UserWatchlist movie, CancellationToken token = default);
        Task<MovieDto?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default);
        Task<IEnumerable<MovieDto>> GetAllAsync(string userId, bool isAdmin = false, CancellationToken token = default);
        Task<UserWatchlist?> UpdateAsync(UserWatchlist movie, Guid? userId = default, CancellationToken token = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
        Task<int> GetCountAsync(string? title, string? yearOfRelease, CancellationToken token = default);
        Task<ResponseModel<string>> AddMovieInWatchlistAsync(UserWatchlist userWatchlist);
    }
}
