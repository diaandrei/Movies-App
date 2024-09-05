using System.Text.Json;
using Microsoft.Extensions.Logging;
using Movies.Application.Models;
using Movies.Contracts.Responses;

namespace Movies.Application.Services
{
    public class OmdbService : IOmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OmdbService> _logger;
        private readonly string _apiKey;

        public OmdbService(ILogger<OmdbService> logger, HttpClient client, string apiKey)
        {
            _logger = logger;
            _httpClient = client;
            _apiKey = apiKey;
        }

        public async Task<ResponseModel<OmdbResponse>> GetMovieAsync(string title, string year, CancellationToken token)
        {
            var res = new ResponseModel<OmdbResponse>
            {
                Title = "Something went wrong.",
                Success = false
            };

            var url = $"http://www.omdbapi.com/?t={title}&y={year}&apikey={_apiKey}";
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