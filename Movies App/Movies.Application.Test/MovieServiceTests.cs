using Microsoft.Extensions.Logging;
using Moq;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Responses;
using FluentValidation;
using AutoMapper;

namespace Movies.Application.Test
{
    [TestFixture]
    public class MovieServiceTests
    {
        private Mock<IMovieRepository> _movieRepositoryMock;
        private Mock<IValidator<Movie>> _movieValidatorMock;
        private Mock<IRatingRepository> _ratingRepositoryMock;
        private Mock<IValidator<GetAllMoviesOptions>> _optionsValidatorMock;
        private Mock<IOmdbService> _omdbServiceMock;
        private Mock<ILogger<MovieService>> _loggerMock;
        private Mock<IMapper> _autoMapperMock;
        private MovieService _movieService;

        [SetUp]
        public void Setup()
        {
            _movieRepositoryMock = new Mock<IMovieRepository>();
            _movieValidatorMock = new Mock<IValidator<Movie>>();
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _optionsValidatorMock = new Mock<IValidator<GetAllMoviesOptions>>();
            _omdbServiceMock = new Mock<IOmdbService>();
            _loggerMock = new Mock<ILogger<MovieService>>();
            _autoMapperMock = new Mock<IMapper>();

            _movieService = new MovieService(
                _movieRepositoryMock.Object,
                _movieValidatorMock.Object,
                _ratingRepositoryMock.Object,
                _optionsValidatorMock.Object,
                _omdbServiceMock.Object,
                _loggerMock.Object,
                _autoMapperMock.Object);
        }

        [Test]
        public async Task CreateAsync_WhenMovieAlreadyExists_ReturnsFailureResponse()
        {
            // Arrange
            var movie = new Movie { Title = "Inception", YearOfRelease = "2010" };
            var cancellationToken = new CancellationToken();

            _omdbServiceMock.Setup(x => x.GetMovieAsync(movie.Title, movie.YearOfRelease, cancellationToken))
                .ReturnsAsync(new ResponseModel<OmdbResponse>
                {
                    Success = true,
                    Content = new OmdbResponse { Title = "Inception", Year = "2010" }
                });

            _movieRepositoryMock.Setup(x => x.GetMovieByTitle(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(true);

            // Act
            var response = await _movieService.CreateAsync(movie, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Title, Is.EqualTo("Title already exist."));
        }

        [Test]
        public async Task CreateAsync_WhenOmdbMovieNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var movie = new Movie { Title = "Nonexistent Movie", YearOfRelease = "2010" };
            var cancellationToken = new CancellationToken();

            _omdbServiceMock.Setup(x => x.GetMovieAsync(movie.Title, movie.YearOfRelease, cancellationToken))
                .ReturnsAsync(new ResponseModel<OmdbResponse>
                {
                    Success = false,
                    Content = null
                });

            // Act
            var response = await _movieService.CreateAsync(movie, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Title, Is.EqualTo("The title does not exist."));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The response is null or title is empty for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task CreateAsync_WhenValidMovie_ReturnsSuccessResponse()
        {
            // Arrange
            var movie = new Movie { Title = "Inception", YearOfRelease = "2010" };
            var omdbResponse = new OmdbResponse { Title = "Inception", Year = "2010", Genre = "Sci-Fi", Actors = "Leonardo DiCaprio" };
            var cancellationToken = new CancellationToken();

            _omdbServiceMock.Setup(x => x.GetMovieAsync(movie.Title, movie.YearOfRelease, cancellationToken))
                .ReturnsAsync(new ResponseModel<OmdbResponse>
                {
                    Success = true,
                    Content = omdbResponse
                });

            _movieRepositoryMock.Setup(x => x.GetMovieByTitle(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(false);

            var validationResult = new FluentValidation.Results.ValidationResult();
            _movieValidatorMock.Setup(x => x.ValidateAsync(movie, cancellationToken))
                .ReturnsAsync(validationResult);

            _movieRepositoryMock.Setup(x => x.CreateAsync(movie, It.IsAny<List<Genre>>(), It.IsAny<List<Cast>>(), It.IsAny<IEnumerable<OmdbRatingResponse>>(), cancellationToken))
                .ReturnsAsync(true);

            // Act
            var response = await _movieService.CreateAsync(movie, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.True);
            Assert.That(response.Title, Is.EqualTo($"{movie.Title} title created Successfully"));
            Assert.That(response.Content, Is.EqualTo(movie.Id.ToString()));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully created title")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }


        [Test]
        public async Task GetByIdAsync_WhenMovieExists_ReturnsMovie()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var movie = new Movie { Id = movieId, Title = "Inception" };
            var cancellationToken = new CancellationToken();

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, false, null!, cancellationToken))
                .ReturnsAsync(movie);

            _ratingRepositoryMock.Setup(x => x.GetAvgUserMovieRatingAsync(movieId, cancellationToken))
                .ReturnsAsync(4.5m);

            // Act
            var result = await _movieService.GetByIdAsync(movieId, false, null!, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Inception"));
            Assert.That(result.UserRating, Is.EqualTo(4.5m));
        }

        [Test]
        public async Task GetByIdAsync_WhenExceptionThrown_LogsErrorAndThrows()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, false, null!, cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _movieService.GetByIdAsync(movieId, false, null!, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo("Database error"));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while retrieving title by ID")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task GetTopMovieAsync_WhenCalled_ReturnsTopMoviesList()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var movies = new List<Movie> { new Movie { Id = Guid.NewGuid(), Title = "Inception" } };

            _movieRepositoryMock.Setup(x => x.GetTopMovieAsync(false, null!, cancellationToken))
                .ReturnsAsync(movies);

            _ratingRepositoryMock.Setup(x => x.GetAvgUserMovieRatingAsync(It.IsAny<Guid>(), cancellationToken))
                .ReturnsAsync(4.5m);

            // Act
            var response = await _movieService.GetTopMovieAsync(false, null!, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.True);
            Assert.That(response.Content.Count(), Is.EqualTo(1));
            Assert.That(response.Content.First().Title, Is.EqualTo("Inception"));
            Assert.That(response.Content.First().UserRating, Is.EqualTo(4.5m));
        }

        [Test]
        public async Task DeleteByIdAsync_WhenMovieExists_ReturnsTrue()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            _movieRepositoryMock.Setup(x => x.DeleteByIdAsync(movieId, cancellationToken))
                .ReturnsAsync(true);

            // Act
            var result = await _movieService.DeleteByIdAsync(movieId, cancellationToken);

            // Assert
            Assert.That(result, Is.True);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully deleted title with ID")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public void DeleteByIdAsync_WhenMovieDoesNotExist_ThrowsException()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            _movieRepositoryMock.Setup(x => x.DeleteByIdAsync(movieId, cancellationToken))
                .ReturnsAsync(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _movieService.DeleteByIdAsync(movieId, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo($"Title with ID '{movieId}' does not exist."));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Title with ID: {movieId} does not exist.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while deleting title by ID")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}