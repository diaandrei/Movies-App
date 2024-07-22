using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IGenreRepository
{
    Task<Genre?> GetGenreAsync(Guid genreId, CancellationToken token = default);
    Task AddGenre(Genre genre, CancellationToken token = default);
    Task<IEnumerable<Genre>> GetAllGenres();
    Task<List<Genre>> GetGenreByMovie(Guid movieId, CancellationToken token = default);
}