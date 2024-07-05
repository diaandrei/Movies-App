using Dapper;
using Microsoft.EntityFrameworkCore;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MoviesDbContext _context;

    public MovieRepository(MoviesDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(token);

        return true;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default) =>
     await _context.Movies.FirstOrDefaultAsync(x => x.Id == id, token);

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        var query = _context.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(options.Title))
        {
            query = query.Where(x => x.Title.Contains(options.Title));
        }

        if (options.YearOfRelease.HasValue)
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
            await _context.SaveChangesAsync(token);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id, token);
        if (movie == null) return false;

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(token);

        return true;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id, token);
        return movie != null;
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
    {
        var query = _context.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(x => x.Title.Contains(title));
        }
        if (yearOfRelease.HasValue)
        {
            query = query.Where(x => x.YearOfRelease == yearOfRelease);
        }
        return await query.CountAsync(token);
    }
}
