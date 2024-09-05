using Movies.Application.Models;
using Movies.Contracts.Responses;

namespace Movies.Application.Repositories
{
    public interface IMovieRepository
    {
        Task<bool> CreateAsync(Movie movie, List<Genre> genres, List<Cast> casts, IEnumerable<OmdbRatingResponse> ombdRating, CancellationToken token = default);
        Task<Movie> GetByIdAsync(Guid id, bool isAdmin = false, string userId = null!, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, bool isAdmin, string userId = null!, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetTopMovieAsync(bool isAdmin = false, string userId = null!, CancellationToken token = default);
        Task<bool> CreateTopMovieAsync(List<TopMovie> movies, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetMostRecentMovieAsync(bool isAdmin = false, string userId = null!, CancellationToken token = default);
        Task<bool> UpdateAsync(Movie movie, CancellationToken token = default);
        Task<bool> GetMovieByTitle(string title, CancellationToken token = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);
        Task<int> GetCountAsync(string? title, string? yearOfRelease, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetSearchedMoviesAsync(string? textToSearchMovie, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetMoviesByIdsAsync(List<Guid> movieIds, CancellationToken token = default);
    }
}
