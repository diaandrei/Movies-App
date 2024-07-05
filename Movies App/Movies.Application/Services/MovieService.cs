using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;


namespace Movies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IValidator<Movie> _movieValidator;
        private readonly IRatingRepository _ratingRepository;
        private readonly IValidator<GetAllMoviesOptions> _optionsValidator;
        private readonly IOmdbService _omdbService;

        public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator, IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> optionsValidator, IOmdbService omdbService)
        {
            _movieRepository = movieRepository;
            _movieValidator = movieValidator;
            _ratingRepository = ratingRepository;
            _optionsValidator = optionsValidator;
            _omdbService = omdbService;
        }

        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            var omdbResponse = await _omdbService.GetMovieAsync(movie.Title, movie.YearOfRelease.ToString(), token);

            if (omdbResponse == null || string.IsNullOrEmpty(omdbResponse.Title))
            {
                throw new ValidationException("The movie does not exist.");
            }

            movie = movie.PopulateValuesFromOmdb(omdbResponse);
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);

            return await _movieRepository.CreateAsync(movie, token);
        }


        public Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
        {
            return _movieRepository.GetByIdAsync(id, userId, token);
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
        {
            await _optionsValidator.ValidateAndThrowAsync(options, token);
            return await _movieRepository.GetAllAsync(options, token);
        }

        public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default)
        {
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);
            var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, token);

            if (!movieExists)
            {
                return null;
            }

            await _movieRepository.UpdateAsync(movie, token);

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

        public Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            return _movieRepository.DeleteByIdAsync(id, token);
        }

        public Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
        {
            return _movieRepository.GetCountAsync(title, yearOfRelease, token);
        }
    }
}
