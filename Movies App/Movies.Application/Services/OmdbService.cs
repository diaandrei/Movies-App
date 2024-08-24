using System.Text.Json;
using Microsoft.Extensions.Logging;
using Movies.Application.Models;
using Movies.Contracts.Responses;

namespace Movies.Application.Services
{
    public class OmdbService : IOmdbService
    {
        private HttpClient _httpClient;
        private readonly ILogger<OmdbService> _logger;

        public OmdbService(ILogger<OmdbService> logger)
        {
            _logger = logger;
        }

        public async Task<ResponseModel<OmdbResponse>> GetMovieAsync(string title, string year, CancellationToken token)
        {
            var res = new ResponseModel<OmdbResponse>
            {
                Title = "Something went wrong.",
                Success = false
            };
            using (_httpClient = new HttpClient())
            {
                var url = $"http://www.omdbapi.com/?t={title}&y={year}&apikey=b4de2ce9";
                var response = await _httpClient.GetAsync(url, token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get title from OMDB API. Status code: {StatusCode}", response.StatusCode);
                    return null!;
                }

                var content = await response.Content.ReadAsStringAsync(token);
                var omdbResponse = JsonSerializer.Deserialize<OmdbResponse>(content);

                if (omdbResponse.Title != null)
                {
                    res.Content = omdbResponse;
                    res.Title = "Title successfully retrieved from OMDB.";
                    res.Success = true;
                }
                else
                {
                    res.Title = "The title does not exist.";
                    res.Success = false;
                }

                return res;
            }
        }
    }
}