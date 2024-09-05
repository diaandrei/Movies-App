using Movies.Application.Models;
using Movies.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Movies.Application.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ILogger<RatingService> _logger;

        public RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository,
            ILogger<RatingService> logger)
        {
            _ratingRepository = ratingRepository;
            _movieRepository = movieRepository;
            _logger = logger;
        }

        public async Task<ResponseModel<string>> RateMovieAsync(MovieRating movieRating, bool isAdmin, string userId = null!, CancellationToken token = default)
        {
            var response = new ResponseModel<string>
            {
                Title = "Oops! Something went wrong. Please retry in a moment.",
                Success = false
            };

            if (movieRating.Rating is <= 0 or > 5)
            {
                _logger.LogWarning("Invalid rating value: {Rating}. Must be between 1 and 5.", movieRating.Rating);
                response.Title = "Rating must be between 1 and 5";
                return response;
            }

            if (!await _movieRepository.ExistsByIdAsync(movieRating.MovieId, token))
            {
                _logger.LogWarning("Title with ID: {MovieId} does not exist.", movieRating.MovieId);
                response.Title = "Title does not exist";
                return response;
            }

            try
            {
                var isMovieRated = await _ratingRepository.IsMovieRatedAsync(movieRating.MovieId, movieRating.UserId, token);

                if (isMovieRated)
                {
                    if (movieRating.Id != null)
                    {
                        var alreadyRatedUserId = await _ratingRepository.MovieRatedAsync(movieRating.Id,
                            movieRating.MovieId, movieRating.UserId, token);
                        var alreadyRatedAdminUserId = userId != null
                            ? await _ratingRepository.MovieRatedAsync(movieRating.Id, movieRating.MovieId, userId,
                                token)
                            : null;

                        if (alreadyRatedUserId != null)
                        {
                            var updateResult = await _ratingRepository.UpdateMovieRatedAsync(movieRating, token);

                            if (updateResult)
                            {
                                _logger.LogInformation(
                                    "Updated title rating for title {MovieId} to {Rating} by user {UserId}.",
                                    movieRating.MovieId, movieRating.Rating, movieRating.UserId);
                                response.Title = "Title rating updated successfully.";
                                response.Success = true;

                                return response;
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Failed to update title rating for title {MovieId} by user {UserId}.",
                                    movieRating.MovieId, movieRating.UserId);
                                response.Title = "Title already rated.";
                                return response;
                            }
                        }
                        else if (alreadyRatedAdminUserId != null && isAdmin)
                        {
                            var updateResult = await _ratingRepository.UpdateMovieRatedAsync(movieRating, token);

                            if (updateResult)
                            {
                                _logger.LogInformation(
                                    "Updated title rating for title {MovieId} to {Rating} by user {UserId}.",
                                    movieRating.MovieId, movieRating.Rating, movieRating.UserId);
                                response.Title = "Title rating updated successfully.";
                                response.Success = true;
                                return response;
                            }
                            else
                            {
                                _logger.LogWarning("Failed to update title rating for title {MovieId} by user {UserId}.",
                                    movieRating.MovieId, movieRating.UserId);
                                response.Title = "Movie already rated.";
                                return response;
                            }
                        }
                    }
                    else
                    {
                        response.Success = false;
                        response.Title = "Title already rated";
                    }
                }
                else
                {
                    var addResult = await _ratingRepository.RateMovieAsync(movieRating, token);

                    if (addResult)
                    {
                        _logger.LogInformation("User {UserId} rated title {MovieId} with rating {Rating}.", movieRating.UserId, movieRating.MovieId, movieRating.Rating);
                        response.Title = "Title rated successfully.";
                        response.Success = true;
                        return response;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to add title rating for title {MovieId} by user {UserId}.", movieRating.MovieId, movieRating.UserId);
                        response.Title = "Failed to add title rating.";
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the rating for title {MovieId} by user {UserId}.", movieRating.MovieId, movieRating.UserId);
                response.Title = "An error occurred while processing the rating for the title.";
                return response;
            }
            return response;
        }

        public async Task<ResponseModel<string>> DeleteRatingAsync(Guid movieId, string userId, CancellationToken token = default)
        {
            var response = new ResponseModel<string>
            {
                Title = "Oops! Something went wrong. Please retry in a moment.",
                Success = false
            };

            try
            {
                var result = await _ratingRepository.DeleteRatingAsync(movieId, token);
                if (result)
                {
                    _logger.LogInformation("User {UserId} deleted rating for title {MovieId}.", userId, movieId);
                    response.Title = "User rating deleted successfully.";
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting rating for title {MovieId} by user {UserId}.",
                    movieId, userId);
                response.Title = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel<IEnumerable<MovieRating>>> GetRatingsForUserAsync(string userId, CancellationToken token = default)
        {
            var response = new ResponseModel<IEnumerable<MovieRating>>
            {
                Title = "Oops! Something went wrong. Please retry in a moment.",
                Success = false
            };

            try
            {
                var ratings = await _ratingRepository.GetRatingsForUserAsync(userId, token);

                foreach (var rating in ratings)
                {
                    rating.Movie = await _movieRepository.GetByIdAsync(rating.MovieId, token: token);
                }

                if (ratings != null)
                {
                    _logger.LogInformation("Retrieved ratings for user {UserId}.", userId);
                    response.Title = "Title rating list";
                    response.Success = true;
                    response.Content = ratings;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving ratings for user {UserId}.", userId);
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }
    }
}