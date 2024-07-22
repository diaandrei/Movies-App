using System.Text.Json;
using Microsoft.Extensions.Logging;
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

        public async Task<OmdbResponse> GetMovieAsync(string title, string year, CancellationToken token)
        {
            using (_httpClient = new HttpClient())
            {
                var url = $"http://www.omdbapi.com/?t={title}&y={year}&apikey=b4de2ce9";
                var response = await _httpClient.GetAsync(url, token);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get movie from OMDB API. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                var content = await response.Content.ReadAsStringAsync();
                var omdbResponse = JsonSerializer.Deserialize<OmdbResponse>(content);
                if (omdbResponse.Title is null)
                {
                    throw new ArgumentException("The movie does not exist.");
                }
                return omdbResponse;
            }
        }
    }
}
