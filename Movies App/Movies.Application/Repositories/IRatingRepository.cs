using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public interface IRatingRepository
    {
        Task<bool> RateMovieAsync(Guid movieId, decimal rating, Guid userId, CancellationToken token = default);
        Task<decimal> GetRatingAsync(Guid movieId, CancellationToken token = default);
        Task<MovieRating> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default);
        Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default);
        Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default);
    }
}