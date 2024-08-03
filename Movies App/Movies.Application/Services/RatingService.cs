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

        public async Task<ResponseModel<string>> RateMovieAsync(MovieRating movieRating, bool isAdmin, CancellationToken token = default)
        {
            var response = new ResponseModel<string>
            {
                Title = "Oops! Something went wrong. Please retry in a moment.",
                Success = false
            };

            if (movieRating.Rating <= 0 || movieRating.Rating > 5)
            {
                _logger.LogWarning("Invalid rating value: {Rating}. Must be between 1 and 5.", movieRating.Rating);
                response.Title = "Rating must be between 1 and 5";
                return response;
            }

            if (!await _movieRepository.ExistsByIdAsync(movieRating.MovieId, token))
            {
                _logger.LogWarning("Movie with ID: {MovieId} does not exist.", movieRating.MovieId);
                response.Title = "Movie does not exist";
                return response;
            }

            var isMovieRatedByUser = await _ratingRepository.IsMovieRatedAsync(movieRating.MovieId, movieRating.UserId, token);
            if (isMovieRatedByUser)
            {
                _logger.LogWarning("Movie with ID: {MovieId} is already rated by user {UserId}. Admin rights are required to update.", movieRating.MovieId, movieRating.UserId);
                response.Title = "Movie is already rated";
            }

            try
            {
                bool result = false;

                if (isMovieRatedByUser)
                {
                    if (isAdmin)
                    {
                        if (await _ratingRepository.ExistsByIdAsync(movieRating.Id, token))
                        {
                            result = await _ratingRepository.UpdateMovieRatedAsync(movieRating, token);
                            if (result)
                            {
                                _logger.LogInformation("Admin user {UserId} updated movie rating for movie {MovieId} to {Rating}.", movieRating.UserId, movieRating.MovieId, movieRating.Rating);
                                response.Title = "Movie rating updated successfully.";
                                response.Success = true;
                                return response;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} is not an admin and cannot update rating for movie {MovieId}.", movieRating.UserId, movieRating.MovieId);
                        response.Title = "User cannot update rating";
                        return response;
                    }
                }
                else
                {
                    result = await _ratingRepository.RateMovieAsync(movieRating, token);
                    if (result)
                    {
                        _logger.LogInformation("User {UserId} rated movie {MovieId} with rating {Rating}.", movieRating.UserId, movieRating.MovieId, movieRating.Rating);
                        response.Title = "Movie rated successfully.";
                        response.Success = true;
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the rating for movie {MovieId} by user {UserId}.", movieRating.MovieId, movieRating.UserId);
                response.Title = "An error occurred while processing the rating for the movie.";
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
                    _logger.LogInformation("User {UserId} deleted rating for movie {MovieId}.", userId, movieId);
                    response.Title = "User rating deleted successfully.";
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting rating for movie {MovieId} by user {UserId}.",
                    movieId, userId);
                response.Title = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel<IEnumerable<MovieRating>>> GetRatingsForUserAsync(string userId,
            CancellationToken token = default)
        {
            var response = new ResponseModel<IEnumerable<MovieRating>>
            {
                Title = "Oops! Something went wrong. Please retry in a moment.",
                Success = false
            };
            try
            {
                var ratings = await _ratingRepository.GetRatingsForUserAsync(userId, token);
                if (ratings != null)
                {
                    _logger.LogInformation("Retrieved ratings for user {UserId}.", userId);
                    response.Title = "Movie rating list";
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