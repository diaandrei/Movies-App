using FluentValidation;
using FluentValidation.Results;
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

        public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken token = default)
        {
            if (rating is <= 0 or > 5)
            {
                _logger.LogWarning("Invalid rating value: {Rating}. Must be between 1 and 5.", rating);
                throw new ValidationException(new[]
                {
                    new ValidationFailure
                    {
                        PropertyName = "Rating",
                        ErrorMessage = "Rating must be between 1 and 5"
                    }
                });
            }

            var movieExists = await _movieRepository.ExistsByIdAsync(movieId, token);
            if (!movieExists)
            {
                _logger.LogWarning("Movie with ID: {MovieId} does not exist.", movieId);
                return false;
            }

            try
            {
                var result = await _ratingRepository.RateMovieAsync(movieId, rating, userId, token);
                _logger.LogInformation("User {UserId} rated movie {MovieId} with rating {Rating}.", userId, movieId,
                    rating);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rating movie {MovieId} by user {UserId}.", movieId,
                    userId);
                throw new ApplicationException($"An error occurred while rating the movie with ID '{movieId}'.", ex);
            }
        }

        public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            try
            {
                var result = await _ratingRepository.DeleteRatingAsync(movieId, userId, token);
                _logger.LogInformation("User {UserId} deleted rating for movie {MovieId}.", userId, movieId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting rating for movie {MovieId} by user {UserId}.",
                    movieId, userId);
                throw new ApplicationException(
                    $"An error occurred while deleting the rating for movie with ID '{movieId}'.", ex);
            }
        }

        public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
            CancellationToken token = default)
        {
            try
            {
                var ratings = await _ratingRepository.GetRatingsForUserAsync(userId, token);
                _logger.LogInformation("Retrieved ratings for user {UserId}.", userId);
                return ratings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving ratings for user {UserId}.", userId);
                throw new ApplicationException(
                    $"An error occurred while retrieving ratings for user with ID '{userId}'.", ex);
            }
        }
    }
}