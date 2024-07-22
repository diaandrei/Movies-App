using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Movies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IValidator<Movie> _movieValidator;
        private readonly IRatingRepository _ratingRepository;
        private readonly IValidator<GetAllMoviesOptions> _optionsValidator;
        private readonly IOmdbService _omdbService;
        private readonly ILogger<MovieService> _logger;

        public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator, IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> optionsValidator, IOmdbService omdbService, ILogger<MovieService> logger)
        {
            _movieRepository = movieRepository;
            _movieValidator = movieValidator;
            _ratingRepository = ratingRepository;
            _optionsValidator = optionsValidator;
            _omdbService = omdbService;
            _logger = logger;
        }

        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            var omdbResponse = await _omdbService.GetMovieAsync(movie.Title, movie.YearOfRelease.ToString(), token);

            if (omdbResponse == null || string.IsNullOrEmpty(omdbResponse.Title))
            {
                _logger.LogWarning("The response is null or title is empty for movie: {Title}", movie.Title);
                throw new ValidationException("The movie does not exist.");
            }

            movie = movie.PopulateValuesFromOmdb(omdbResponse);
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);

            try
            {
                await _movieRepository.CreateAsync(movie, token);
                _logger.LogInformation("Successfully created movie: {Title} (ID: {Id})", movie.Title, movie.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating movie: {Title}", movie.Title);
                throw;
            }
        }

        public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
        {
            try
            {
                var movie = await _movieRepository.GetByIdAsync(id, userId, token);
                _logger.LogInformation("Successfully retrieved movie by ID: {Id}", id);
                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving movie by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
        {
            await _optionsValidator.ValidateAndThrowAsync(options, token);

            try
            {
                var movies = await _movieRepository.GetAllAsync(options, token);
                _logger.LogInformation("Successfully retrieved all movies with options: {Options}", options);
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all movies.");
                throw;
            }
        }

        public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default)
        {
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);

            try
            {
                var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, token);

                if (!movieExists)
                {
                    _logger.LogWarning("Movie with ID: {Id} does not exist.", movie.Id);
                    throw new KeyNotFoundException($"Movie with ID '{movie.Id}' does not exist.");
                }

                await _movieRepository.UpdateAsync(movie, token);
                _logger.LogInformation("Successfully updated movie: {Title} (ID: {Id})", movie.Title, movie.Id);

                if (!userId.HasValue)
                {
                    var rating = await _ratingRepository.GetRatingAsync(movie.Id, token);
                    movie.Rating = rating;
                    return movie;
                }

                var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, token);
                movie.Rating = ratings.Rating;
                movie.UserRating = ratings.Rating;
                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating movie: {Title}", movie.Title);
                throw;
            }
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            try
            {
                var success = await _movieRepository.DeleteByIdAsync(id, token);
                if (!success)
                {
                    _logger.LogWarning("Movie with ID: {Id} does not exist.", id);
                    throw new KeyNotFoundException($"Movie with ID '{id}' does not exist.");
                }
                _logger.LogInformation("Successfully deleted movie with ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting movie by ID: {Id}", id);
                throw;
            }
        }

        public async Task<int> GetCountAsync(string? title, string? yearOfRelease, CancellationToken token = default)
        {
            try
            {
                var count = await _movieRepository.GetCountAsync(title, yearOfRelease, token);
                _logger.LogInformation("Successfully retrieved movie count with title: {Title} and year of release: {YearOfRelease}", title, yearOfRelease);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the movie count.");
                throw;
            }
        }
    }
}
