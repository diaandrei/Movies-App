using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Movies.Application.DataTransferObjects;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services
{
    public class UserWatchlistService : IUserWatchlistService
    {
        private readonly IUserWatchlistRepository _userWatchlistRepository;
        private readonly ILogger<UserWatchlistService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserWatchlistService(IUserWatchlistRepository userWatchlistRepository, ILogger<UserWatchlistService> logger, UserManager<ApplicationUser> userManager)
        {
            _userWatchlistRepository = userWatchlistRepository;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            try
            {
                var success = await _userWatchlistRepository.DeleteByIdAsync(id, token);
                if (!success)
                {
                    _logger.LogWarning("Title with ID: {Id} does not exist.", id);
                    throw new KeyNotFoundException($"Movie with ID '{id}' does not exist.");
                }
                _logger.LogInformation("Successfully deleted title with ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting title by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<MovieDto>> GetAllAsync(string userId, bool isAdmin = false, CancellationToken token = default)
        {
            try
            {
                var movies = await _userWatchlistRepository.GetAllAsync(isAdmin, userId, token);
                _logger.LogInformation("Successfully retrieved all titles with options: {Options}");
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all titles.");
                throw;
            }
        }

        public async Task<UserWatchlist?> UpdateAsync(UserWatchlist movie, Guid? userId = null, CancellationToken token = default)
        {
            try
            {
                var movieDetail = await _userWatchlistRepository.GetByIdAsync(movie.Id, token: token);

                if (movieDetail == null)
                {
                    _logger.LogWarning("Title with ID: {Id} does not exist.", movie.Id);
                    throw new KeyNotFoundException($"Title with ID '{movie.Id}' does not exist.");
                }
                movieDetail.UpdatedAt = movie.UpdatedAt;
                movieDetail.IsActive = false;
                movieDetail.UserId = movie.UserId;
                await _userWatchlistRepository.UpdateAsync(movieDetail, token);
                _logger.LogInformation("Successfully updated title: {Title} (ID: {Id})");

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating title: {Title}");
                throw;
            }
        }

        public async Task<ResponseModel<string>> AddMovieInWatchlistAsync(UserWatchlist userWatchlist)
        {
            var response = new ResponseModel<string>
            {
                Title = "Something went wrong.",
                Success = false
            };
            try
            {
                var movieAdded = await _userWatchlistRepository.AddMovieInWatchlistAsync(userWatchlist);

                if (movieAdded != null)
                {
                    var isFirstTimeUser = await IsFirstTimeUserInWatchlistAsync(userWatchlist.UserId);

                    if (isFirstTimeUser)
                    {
                        var user = await _userManager.FindByIdAsync(userWatchlist.UserId);

                        if (user != null)
                        {
                            user.FirstAddedToWatchlistAt = movieAdded.CreatedAt;
                            var updateResult = await _userManager.UpdateAsync(user);

                            if (!updateResult.Succeeded)
                            {
                                _logger.LogWarning("Failed to add title to watchlist for user: {UserId}", userWatchlist.UserId);
                            }
                        }
                    }

                    response.Title = "Successfully added the title to your watchlist";
                    response.Success = true;
                    _logger.LogInformation("Successfully added title to watchlist: (movieId: {MovieId}, userId: {UserId})", userWatchlist.MovieId, userWatchlist.UserId);
                }

            }
            catch (Exception ex)
            {
                response.Title = ex.Message;
                response.Success = false;
                _logger.LogError(ex, "An error occurred while adding title to watchlist: {MovieId}", userWatchlist.MovieId);
                throw;
            }
            return response;
        }

        private async Task<bool> IsFirstTimeUserInWatchlistAsync(string userId, CancellationToken token = default)
        {
            var count = await _userWatchlistRepository.CountUserWatchlistAsync(userId, token);
            return count;
        }
    }
}
