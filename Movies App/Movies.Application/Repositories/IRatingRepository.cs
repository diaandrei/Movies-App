using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public interface IRatingRepository
    {
        Task<bool> RateMovieAsync(MovieRating movieRating, CancellationToken token = default);
        Task<bool> IsMovieRatedAsync(Guid movieId, string userId, CancellationToken token = default);
        Task<bool> UpdateMovieRatedAsync(MovieRating movieRating, CancellationToken token = default);
        Task<decimal> GetRatingAsync(Guid movieId, CancellationToken token = default);
        Task<MovieRating> GetRatingAsync(Guid movieId, string userId, CancellationToken token = default);
        Task<bool> DeleteRatingAsync(Guid movieId, CancellationToken token = default);
        Task<decimal> GetAvgUserMovieRatingAsync(Guid movieId, CancellationToken token = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);
        Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(string userId, CancellationToken token = default);
    }
}