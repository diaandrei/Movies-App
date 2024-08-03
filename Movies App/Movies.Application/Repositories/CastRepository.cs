using Microsoft.EntityFrameworkCore;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class CastRepository : ICastRepository
    {
        private readonly MoviesDbContext _dbcontext;

        public CastRepository(MoviesDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Cast?> GetCastAsync(Guid castId, CancellationToken token = default) =>
            await _dbcontext.Casts
                .Include(x => x.Movies)
                .FirstOrDefaultAsync(x => x.Id == castId, token);

        public async Task AddCast(Cast cast, CancellationToken token = default)
        {
            _dbcontext.Casts.Add(cast);
            await _dbcontext.SaveChangesAsync(token);
        }

        public async Task<IEnumerable<Cast>> GetAllCasts()
        {
            return await _dbcontext.Casts.ToListAsync();
        }

        public async Task<List<Cast>> GetCastByMovie(Guid movieId, CancellationToken token = default)
        {
            return new List<Cast>();
        }
    }
}