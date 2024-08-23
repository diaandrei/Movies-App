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

        public async Task<bool> RateMovieAsync(MovieRating movieRating, CancellationToken token = default)
        {
            _dbcontext.MovieRatings.Add(new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = movieRating.MovieId,
                Rating = Math.Round(movieRating.Rating, 2),
                UserId = movieRating.UserId,
                CreatedAt = movieRating.CreatedAt,
                IsUserRated = true
            });
            await _dbcontext.SaveChangesAsync(token);
            return true;
        }

        public async Task<decimal> GetRatingAsync(Guid movieId, CancellationToken token = default) =>
            await _dbcontext.MovieRatings.Where(x => x.MovieId == movieId).Select(x => x.Rating).AverageAsync(token);

        public async Task<MovieRating> GetRatingAsync(Guid movieId, string userId, CancellationToken token = default)
        {
            try
            {
                var result = await _dbcontext.MovieRatings
                .Where(x => x.MovieId == movieId && x.UserId == userId)
                .Select(x => new { x.Rating, UserRating = (decimal)x.Rating })
                .FirstOrDefaultAsync(token);

                return new MovieRating
                {
                    MovieId = movieId,
                    UserId = userId
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteRatingAsync(Guid ratingId, CancellationToken token = default)
        {
            var ratings = await _dbcontext.MovieRatings
                .Where(x => x.Id == ratingId)
                .FirstOrDefaultAsync(token);

            _dbcontext.MovieRatings.RemoveRange(ratings);
            return await _dbcontext.SaveChangesAsync(token) > 0;
        }

        public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(string userId, CancellationToken token = default)
        {
            return await _dbcontext.MovieRatings
                .Include(r => r.Movie)
                .Where(x => x.UserId == userId)
                .ToListAsync(token);
        }


        public async Task<bool> IsMovieRatedAsync(Guid movieId, string userId, CancellationToken token = default)
        {
            return await _dbcontext.MovieRatings
                .AnyAsync(x => x.MovieId == movieId && x.UserId == userId, token);
        }

        public async Task<string> MovieRatedAsync(Guid Id, Guid movieId, string userId, CancellationToken token = default)
        {
            var isRated = await _dbcontext.MovieRatings
                .AnyAsync(x => x.Id == Id && x.MovieId == movieId && x.UserId == userId, token);

            if (isRated)
            {
                return userId;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> UpdateMovieRatedAsync(MovieRating movieRating, CancellationToken token = default)
        {
            try
            {
                var existingMovieRating = await _dbcontext.MovieRatings
                    .FirstOrDefaultAsync(mr => mr.Id == movieRating.Id, token);

                if (existingMovieRating == null)
                {
                    return false;
                }
                existingMovieRating.Rating = movieRating.Rating;
                existingMovieRating.UpdatedAt = DateTime.UtcNow;
                await _dbcontext.SaveChangesAsync(token);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<decimal> GetAvgUserMovieRatingAsync(Guid movieId, CancellationToken token = default)
        {
            try
            {
                var ratings = await _dbcontext.MovieRatings
                    .Where(r => r.MovieId == movieId)
                    .Select(r => r.Rating)
                    .ToListAsync(token);

                if (ratings.Any())
                {
                    var averageRating = ratings.Average();
                    return Math.Round(averageRating, 2);
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
        {
            var movie = await _dbcontext.MovieRatings.FirstOrDefaultAsync(x => x.Id == id, token);
            return movie != null;
        }
    }
}
