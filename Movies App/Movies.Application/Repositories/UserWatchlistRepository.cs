using Microsoft.EntityFrameworkCore;
using Movies.Application.Database;
using Movies.Application.DataTransferObjects;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class UserWatchlistRepository : IUserWatchlistRepository
    {
        private readonly MoviesDbContext _dbContext;

        public UserWatchlistRepository(MoviesDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            var userWatchlistMovie = await _dbContext.UserWatchlists.FirstOrDefaultAsync(x => x.Id == id, token);
            if (userWatchlistMovie == null) return false;

            _dbContext.UserWatchlists.Remove(userWatchlistMovie);
            await _dbContext.SaveChangesAsync(token);

            return true;
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
        {
            var movie = await _dbContext.UserWatchlists.FirstOrDefaultAsync(x => x.Id == id, token);
            return movie != null;
        }

        public async Task<IEnumerable<MovieDto>> GetAllAsync(bool isAdmin, string userId = null!, CancellationToken token = default)
        {
            var userWatchlists = await _dbContext.UserWatchlists
                .Where(uw => uw.UserId == userId)
                .ToListAsync(token);

            var movieIds = userWatchlists.Select(uw => uw.MovieId).ToList();

            if (!movieIds.Any())
            {
                return new List<MovieDto>();
            }

            var movies = await _dbContext.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToListAsync(token);

            var casts = await _dbContext.MovieCast
            .Where(mc => movieIds.Contains(mc.MovieId))
            .Include(mc => mc.Cast)
            .ToListAsync(token);

            var genres = await _dbContext.MovieGenres
                .Where(mg => movieIds.Contains(mg.MovieId))
                .Include(mg => mg.Genre)
                .ToListAsync(token);

            var externalRatings = await _dbContext.ExternalRatings
                .Where(er => movieIds.Contains(er.MovieId))
                .ToListAsync(token);

            var omdbRatings = await _dbContext.OmdbRatings
                .Where(or => movieIds.Contains(or.MovieId))
                .ToListAsync(token);

            var movieRatings = await _dbContext.MovieRatings
                .Where(mr => movieIds.Contains(mr.MovieId) && mr.UserId == userId)
                .ToListAsync(token);

            var userWatchlist = await _dbContext.UserWatchlists
            .Where(uw => movieIds.Contains(uw.MovieId) && uw.UserId == userId)
            .ToListAsync(token);

            ApplicationUser? user = null;

            if (!string.IsNullOrEmpty(userId))
            {
                user = await _dbContext.Users
                .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync(token);
            }
            var movieDTOs = movies.Select(movie => new MovieDto
            {
                Id = movie.Id,
                UserWatchlistId = userWatchlists
                    .Where(uw => uw.MovieId == movie.Id)
                    .Select(uw => uw.Id)
                    .FirstOrDefault(),
                Title = movie.Title,
                Released = movie.Released,
                Runtime = movie.Runtime,
                YearOfRelease = movie.YearOfRelease,
                Rated = movie.Rated,
                Plot = movie.Plot,
                Awards = movie.Awards,
                Poster = movie.Poster,
                TotalSeasons = movie.TotalSeasons,
                IsActive = movie.IsActive,
                Rating = movie.Rating,
                UserRating = movie.UserRating,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt,
                Cast = casts.Where(mc => mc.MovieId == movie.Id).Select(mc => new CastDto
                {
                    Id = mc.CastId,
                    Name = mc.Cast.Name,
                    Role = mc.Cast.Role
                }).ToList(),
                Genres = genres.Where(mg => mg.MovieId == movie.Id).Select(mg => new GenreDto
                {
                    Id = mg.GenreId,
                    Name = mg.Genre.Name
                }).ToList(),
                ExternalRatings = externalRatings.Where(er => er.MovieId == movie.Id).Select(er => new ExternalRatingDto
                {
                    Source = er.Source,
                    Rating = er.Rating
                }).ToList(),
                OmdbRatings = omdbRatings.Where(or => or.MovieId == movie.Id).Select(or => new OmdbRatingDto
                {
                    Id = or.Id,
                    Source = or.Source,
                    Value = or.Value
                }).ToList(),
                MovieRatings = movieRatings.Where(mr => mr.MovieId == movie.Id).Select(mr => new MovieRatingDto
                {
                    Id = mr.Id,
                    Rating = mr.Rating,
                }).ToList(),
                ApplicationUser = user!
            }).ToList();

            return movieDTOs;
        }

        public async Task<UserWatchlist?> GetByIdAsync(Guid id, bool isAdmin = false, Guid? userId = null, CancellationToken token = default)
        {
            return await _dbContext.UserWatchlists.FirstOrDefaultAsync(x => x.Id == id, token);
        }

        public async Task<bool> UpdateAsync(UserWatchlist movie, CancellationToken token = default)
        {
            await _dbContext.SaveChangesAsync(token);
            return true;
        }

        public async Task<UserWatchlist> AddMovieInWatchlistAsync(UserWatchlist userWatchlist, CancellationToken token = default)
        {
            var existingWatchlistEntry = await _dbContext.UserWatchlists
                .FirstOrDefaultAsync(uwl => uwl.UserId == userWatchlist.UserId && uwl.MovieId == userWatchlist.MovieId, token);

            if (existingWatchlistEntry == null)
            {
                var movieEntry = _dbContext.UserWatchlists.Add(userWatchlist);
                await _dbContext.SaveChangesAsync(token);
                return movieEntry.Entity;
            }

            return null!;
        }

        public async Task<bool> CountUserWatchlistAsync(string userId, CancellationToken token = default)
        {
            var count = await _dbContext.UserWatchlists
            .CountAsync(uwl => uwl.UserId == userId, token);

            return count == 1;
        }
    }
}
