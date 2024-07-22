using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MoviesDbContext _dbContext;

    public MovieRepository(MoviesDbContext dbcontext)
    {
        _dbContext = dbcontext;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default) =>
     await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == id, token);

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        var query = _dbContext.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(options.Title))
        {
            query = query.Where(x => x.Title.Contains(options.Title));
        }

        if (!string.IsNullOrEmpty(options.YearOfRelease))
        {
            query = query.Where(x => x.YearOfRelease == options.YearOfRelease);
        }

        query = options.SortField switch
        {
            SortField.Title => options.SortOrder == SortOrder.Ascending ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
            _ => query
        };

        var result = await query.Skip((options.Page - 1) * options.PageSize).Take(options.PageSize).ToListAsync(token);

        return result;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(token);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == id, token);
        if (movie == null) return false;

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync(token);

        return true;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == id, token);
        return movie != null;
    }

    public async Task<int> GetCountAsync(string? title, string yearOfRelease, CancellationToken token = default)
    {
        var query = _dbContext.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(x => x.Title.Contains(title));
        }
        if (!string.IsNullOrEmpty(yearOfRelease))
        {
            query = query.Where(x => x.YearOfRelease == yearOfRelease);
        }
        return await query.CountAsync(token);
    }
}
