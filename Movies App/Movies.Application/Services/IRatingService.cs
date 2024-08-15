using Movies.Application.Models;

namespace Movies.Application.Services
{
    public interface IRatingService
    {
        Task<ResponseModel<string>> RateMovieAsync(MovieRating movieRating, bool isAdmin, string userId = null!, CancellationToken token = default);
        Task<ResponseModel<string>> DeleteRatingAsync(Guid movieId, string userId, CancellationToken token = default);
        Task<ResponseModel<IEnumerable<MovieRating>>> GetRatingsForUserAsync(string userId, CancellationToken token = default);
    }
}
