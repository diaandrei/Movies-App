using Microsoft.EntityFrameworkCore;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly MoviesDbContext _dbcontext;

        public GenreRepository(MoviesDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Genre?> GetGenreAsync(Guid genreId, CancellationToken token = default) =>
            await _dbcontext.Genres
                .Include(x => x.Movies)
                .FirstOrDefaultAsync(x => x.Id == genreId, token);

        public async Task AddGenre(Genre genre, CancellationToken token = default)
        {
            _dbcontext.Genres.Add(genre);
            await _dbcontext.SaveChangesAsync(token);
        }

        public async Task<IEnumerable<Genre>> GetAllGenres()
        {
            return await _dbcontext.Genres.ToListAsync();
        }

        public async Task<List<Genre>> GetGenreByMovie(Guid movieId, CancellationToken token = default)
        {
            return await _dbcontext.Movies
                .Include(c => c.Genres)
                .Where(c => c.Id == movieId)
                .SelectMany(c => c.Genres)
                .ToListAsync(token);
        }
    }
}
