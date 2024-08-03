using AutoMapper;
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
        private readonly IMapper _autoMapper;
        private readonly IRatingRepository _ratingRepository;
        private readonly IValidator<GetAllMoviesOptions> _optionsValidator;
        private readonly IOmdbService _omdbService;
        private readonly ILogger<MovieService> _logger;

        public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator,
            IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> optionsValidator,
            IOmdbService omdbService, ILogger<MovieService> logger, IMapper autoMapper)
        {
            _movieRepository = movieRepository;
            _movieValidator = movieValidator;
            _ratingRepository = ratingRepository;
            _optionsValidator = optionsValidator;
            _omdbService = omdbService;
            _logger = logger;
            _autoMapper = autoMapper;
        }

        public async Task<ResponseModel<string>> CreateAsync(Movie movie, CancellationToken token = default)
        {
            var response = new ResponseModel<string>
            {
                Title = "Oops! Something went wrong. Please retry in a moment.",
                Success = false
            };
            var omdbResponse = await _omdbService.GetMovieAsync(movie.Title, movie.YearOfRelease.ToString(), token);

            if (omdbResponse.Success == false || string.IsNullOrEmpty(omdbResponse.Content.Title))
            {
                response.Title = "The movie does not exist.";
                _logger.LogWarning("The response is null or title is empty for movie: {Title}", movie.Title);
                return response;
            }

            var movieExist = await _movieRepository.GetMovieByTitle(omdbResponse.Content.Title);

            if (movieExist)
            {
                response.Success = false;
                response.Title = "Movie already exist.";
                return response;
            }

            var movieWithGenres = movie.PopulateGenresFromOmdb(omdbResponse.Content.Genre);
            var movieWithCast = movie.PopulateCastFromOmdb(omdbResponse.Content.Actors);

            movie = movie.PopulateValuesFromOmdb(omdbResponse.Content);
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);

            try
            {
                await _movieRepository.CreateAsync(movie, movieWithGenres, movieWithCast, omdbResponse.Content.Ratings,
                    token);
                response.Title = $"{movie.Title} movie created Successfully";
                response.Success = true;
                _logger.LogInformation("Successfully created title: {Title} (ID: {Id})", movie.Title, movie.Id);

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
                _logger.LogError(ex, "An error occurred while creating movie: {Title}", movie.Title);
                throw;
            }

            return response;
        }

        public async Task<ResponseModel<string>> CreateTopMovieAsync(List<TopMovie> topMovies,
            CancellationToken token = default)
        {
            var responseModel = new ResponseModel<string>
            {
                Title = "Oops! Something went wrong. Please retry in a moment.",
                Success = false
            };

            try
            {
                if (topMovies.Count < 10)
                {
                    responseModel.Title = "The number of top movies is less than the required minimum of 10.";
                    return responseModel;
                }

                var movieIds = topMovies.Select(tm => tm.MovieId).ToList();
                var existingMovies = await _movieRepository.GetMoviesByIdsAsync(movieIds, token);

                if (existingMovies.Count() == topMovies.Count)
                {
                    var movie = await _movieRepository.CreateTopMovieAsync(topMovies);

                    if (movie)
                    {
                        responseModel.Success = true;
                        responseModel.Title = "Top movies have been successfully added to the list.";
                    }
                }
                else
                {
                    responseModel.Title = "Some movies were not found in the collection.";
                }
            }
            catch (Exception ex)
            {
                responseModel.Title = ex.Message;
            }

            return responseModel;
        }

        public async Task<Movie> GetByIdAsync(Guid id, bool isAdmin, string userId = null,
            CancellationToken token = default)
        {
            try
            {
                var movie = await _movieRepository.GetByIdAsync(id, isAdmin, userId, token);
                var avgUserRating = await _ratingRepository.GetAvgUserMovieRatingAsync(movie.Id);
                movie.UserRating = avgUserRating;
                _logger.LogInformation("Successfully retrieved movie by ID: {Id}", id);
                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving movie by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, bool isFavourite, bool isAdmin,
            string userId = null, CancellationToken token = default)
        {
            await _optionsValidator.ValidateAndThrowAsync(options, token);

            try
            {
                var movies = await _movieRepository.GetAllAsync(options, isAdmin, userId, token);
                foreach (var item in movies)
                {
                    var avgMovieRating = await _ratingRepository.GetAvgUserMovieRatingAsync(item.Id);
                    item.UserRating = avgMovieRating;
                }

                if (isFavourite)
                {
                    var sortedMovies = movies.OrderByDescending(m => m.UserRating).ToList();
                    return sortedMovies;
                }

                _logger.LogInformation("Successfully retrieved all movies with options: {Options}", options);
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all movies.");
                throw;
            }
        }

        public async Task<ResponseModel<IEnumerable<Movie>>> GetTopMovieAsync(bool isAdmin = false,
            string userId = null, CancellationToken token = default)
        {
            var response = new ResponseModel<IEnumerable<Movie>>
            {
                Success = false,
                Title = "Oops! Something went wrong. Please try again."
            };

            try
            {
                var movieList = await _movieRepository.GetTopMovieAsync(isAdmin, userId, token);
                foreach (var item in movieList)
                {
                    var avgMovieRating = await _ratingRepository.GetAvgUserMovieRatingAsync(item.Id);
                    item.UserRating = avgMovieRating;
                }

                response.Content = movieList;
                response.Success = true;
                response.Title = "Top movies list";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve top movies");
                response.Title = "Failed to retrieve top movies.";
            }

            return response;
        }

        public async Task<IEnumerable<Movie>> GetMostRecentMovieAsync(bool isAdmin = false, string userId = null,
            CancellationToken token = default)
        {
            try
            {
                var movies = await _movieRepository.GetMostRecentMovieAsync(isAdmin, userId, token);
                foreach (var item in movies)
                {
                    var avgMovieRating = await _ratingRepository.GetAvgUserMovieRatingAsync(item.Id);
                    item.UserRating = avgMovieRating;
                }

                _logger.LogInformation("Successfully retrieved top movies ");
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all movies.");
                throw;
            }
        }

        public async Task<Movie> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default)
        {
            try
            {
                var movieDetail = await _movieRepository.GetByIdAsync(movie.Id);

                if (movieDetail == null)
                {
                    _logger.LogWarning("Title with ID: {Id} does not exist.", movie.Id);
                    throw new KeyNotFoundException($"Movie with ID '{movie.Id}' does not exist.");
                }

                movieDetail.UpdatedAt = DateTime.UtcNow;
                movieDetail.Plot = movie.Plot;

                await _movieRepository.UpdateAsync(movieDetail, token);
                _logger.LogInformation("Successfully updated title: {Title} (ID: {Id})", movieDetail.Title, movie.Id);

                return movieDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating title: {Title}");
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
                    _logger.LogWarning("Title with ID: {Id} does not exist.", id);
                    throw new KeyNotFoundException($"Title with ID '{id}' does not exist.");
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
                _logger.LogInformation(
                    "Successfully retrieved title count with: {Title} and year of release: {YearOfRelease}",
                    title, yearOfRelease);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the movie count.");
                throw;
            }
        }

        public async Task<IEnumerable<Movie>> GetSearchedMoviesAsync(string? textToSearchMovies)
        {
            try
            {
                var moviesData = await _movieRepository.GetSearchedMoviesAsync(textToSearchMovies);
                _logger.LogInformation($"Successfully retrieved titles with searchText: {textToSearchMovies}",
                    textToSearchMovies);
                return moviesData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for titles.");
                throw;
            }
        }
    }
}