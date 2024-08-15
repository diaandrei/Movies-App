using Movies.Application.Models;

namespace Movies.Application.Services
{
    public interface IMovieService
    {
        Task<ResponseModel<string>> CreateAsync(Movie movie, CancellationToken token = default);
        Task<Movie> GetByIdAsync(Guid id, bool isAdmin, string userId = null, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, bool isFavourite, bool isAdmin, string userId = null, CancellationToken token = default);
        Task<ResponseModel<IEnumerable<Movie>>> GetTopMovieAsync(bool isAdmin = false, string userId = null, CancellationToken token = default);

        Task<ResponseModel<string>> CreateTopMovieAsync(List<TopMovie> topMovies, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetMostRecentMovieAsync(bool isAdmin = false, string userId = null, CancellationToken token = default);
        Task<ResponseModel<Movie>> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
        Task<int> GetCountAsync(string? title, string? yearOfRelease, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetSearchedMoviesAsync(string textToSearchMovie);
    }
}
