using Movies.Application.Models;
using Movies.Contracts.Responses;

namespace Movies.Application.Services
{
    public interface IOmdbService
    {
        Task<ResponseModel<OmdbResponse>> GetMovieAsync(string title, string year, CancellationToken token);
    }
}
