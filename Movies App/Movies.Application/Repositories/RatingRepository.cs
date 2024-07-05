using Microsoft.EntityFrameworkCore;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly MoviesDbContext _dbcontext;

        public RatingRepository(MoviesDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<bool> RateMovieAsync(Guid movieId, decimal rating, Guid userId, CancellationToken token = default)
        {
            _dbcontext.MovieRatings.Add(new MovieRating
            {
                MovieId = movieId,
                Rating = rating,
                UserId = userId
            });
            return await _dbcontext.SaveChangesAsync(token) > 0;
        }

        public async Task<decimal> GetRatingAsync(Guid movieId, CancellationToken token = default) =>
            await _dbcontext.MovieRatings.Where(x => x.MovieId == movieId).Select(x => x.Rating).AverageAsync(token);

        public async Task<MovieRating> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            var result = await _dbcontext.MovieRatings
                .Where(x => x.MovieId == movieId && x.UserId == userId)
                .Select(x => new { x.Rating, UserRating = (decimal)x.Rating })
                .FirstOrDefaultAsync(token);

            var averageRating = await _dbcontext.MovieRatings
                .Where(x => x.MovieId == movieId)
                .AverageAsync(x => x.Rating, token);

            return new MovieRating
            {
                MovieId = movieId,
                Rating = result.Rating,
                UserId = userId
            };
        }

        public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            var ratings = await _dbcontext.MovieRatings
                .Where(x => x.MovieId == movieId && x.UserId == userId)
                .ToListAsync(token);

            _dbcontext.MovieRatings.RemoveRange(ratings);
            return await _dbcontext.SaveChangesAsync(token) > 0;
        }

        public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default) =>
            await _dbcontext.MovieRatings.Where(x => x.UserId == userId).ToListAsync(token);
    }
}
