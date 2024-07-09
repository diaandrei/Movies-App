using Movies.Application.Models;

namespace Movies.Application.Repositories;
public interface ICastRepository
{
    Task<Cast?> GetCastAsync(Guid castId, CancellationToken token = default);
    Task AddCast(Cast cast, CancellationToken token = default);
    Task<IEnumerable<Cast>> GetAllCasts();
    Task<List<Cast>> GetCastByMovie(Guid movieId, CancellationToken token = default);
}