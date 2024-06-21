using Movies.Contracts.Responses;

namespace Movies.Application.Services
{
    public interface IOmdbService
    {
        Task<OmdbResponse> GetMovieAsync(string title, string year, CancellationToken token);
    }
}
