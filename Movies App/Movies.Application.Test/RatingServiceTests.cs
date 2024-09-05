using Microsoft.Extensions.Logging;
using Moq;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application.Test
{
    [TestFixture]
    public class RatingServiceTests
    {
        private Mock<IRatingRepository> _ratingRepositoryMock;
        private Mock<IMovieRepository> _movieRepositoryMock;
        private Mock<ILogger<RatingService>> _loggerMock;
        private RatingService _ratingService;

        [SetUp]
        public void Setup()
        {
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _movieRepositoryMock = new Mock<IMovieRepository>();
            _loggerMock = new Mock<ILogger<RatingService>>();
            _ratingService = new RatingService(_ratingRepositoryMock.Object, _movieRepositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task RateMovieAsync_WhenRatingIsInvalid_ReturnsFailureResponse()
        {
            // Arrange
            var movieRating = new MovieRating { Rating = 6 };
            var cancellationToken = new CancellationToken();

            // Act
            var response = await _ratingService.RateMovieAsync(movieRating, false, null!, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Title, Is.EqualTo("Rating must be between 1 and 5"));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid rating value")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task RateMovieAsync_WhenMovieDoesNotExist_ReturnsFailureResponse()
        {
            // Arrange
            var movieRating = new MovieRating { MovieId = Guid.NewGuid(), Rating = 4 };
            var cancellationToken = new CancellationToken();

            _movieRepositoryMock.Setup(x => x.ExistsByIdAsync(movieRating.MovieId, cancellationToken))
                .ReturnsAsync(false);

            // Act
            var response = await _ratingService.RateMovieAsync(movieRating, false, null!, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Title, Is.EqualTo("Title does not exist"));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Title with ID")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task RateMovieAsync_WhenMovieIsAlreadyRated_ReturnsFailureResponse()
        {
            // Arrange
            var movieRating = new MovieRating
            {
                MovieId = Guid.NewGuid(),
                UserId = "user123",
                Rating = 4
            };
            var cancellationToken = new CancellationToken();

            _movieRepositoryMock.Setup(x => x.ExistsByIdAsync(movieRating.MovieId, cancellationToken))
                .ReturnsAsync(true);
            _ratingRepositoryMock.Setup(x => x.IsMovieRatedAsync(movieRating.MovieId, movieRating.UserId, cancellationToken))
                .ReturnsAsync(true);
            _ratingRepositoryMock.Setup(x => x.MovieRatedAsync(movieRating.Id, movieRating.MovieId, movieRating.UserId, cancellationToken))!
                .ReturnsAsync((string)null!);

            // Act
            var response = await _ratingService.RateMovieAsync(movieRating, false, null!, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Title, Is.EqualTo("Oops! Something went wrong. Please retry in a moment."));
        }


        [Test]
        public async Task RateMovieAsync_WhenRatingIsAddedSuccessfully_ReturnsSuccessResponse()
        {
            // Arrange
            var movieRating = new MovieRating { MovieId = Guid.NewGuid(), UserId = "user123", Rating = 4 };
            var cancellationToken = new CancellationToken();

            _movieRepositoryMock.Setup(x => x.ExistsByIdAsync(movieRating.MovieId, cancellationToken))
                .ReturnsAsync(true);
            _ratingRepositoryMock.Setup(x => x.IsMovieRatedAsync(movieRating.MovieId, movieRating.UserId, cancellationToken))
                .ReturnsAsync(false);
            _ratingRepositoryMock.Setup(x => x.RateMovieAsync(movieRating, cancellationToken))
                .ReturnsAsync(true);

            // Act
            var response = await _ratingService.RateMovieAsync(movieRating, false, null!, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.True);
            Assert.That(response.Title, Is.EqualTo("Title rated successfully."));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User") && v.ToString()!.Contains("rated title")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task DeleteRatingAsync_WhenDeletionIsSuccessful_ReturnsSuccessResponse()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var userId = "user123";
            var cancellationToken = new CancellationToken();

            _ratingRepositoryMock.Setup(x => x.DeleteRatingAsync(movieId, cancellationToken))
                .ReturnsAsync(true);

            // Act
            var response = await _ratingService.DeleteRatingAsync(movieId, userId, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.True);
            Assert.That(response.Title, Is.EqualTo("User rating deleted successfully."));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User") && v.ToString()!.Contains("deleted rating")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task GetRatingsForUserAsync_WhenRatingsExist_ReturnsRatings()
        {
            // Arrange
            var userId = "user123";
            var movieRating = new MovieRating { MovieId = Guid.NewGuid(), UserId = userId, Rating = 4 };
            var cancellationToken = new CancellationToken();
            var ratings = new List<MovieRating> { movieRating };

            _ratingRepositoryMock.Setup(x => x.GetRatingsForUserAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ratings);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieRating.MovieId, false, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Movie { Id = movieRating.MovieId });

            // Act
            var response = await _ratingService.GetRatingsForUserAsync(userId, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.True);
            Assert.That(response.Content, Is.EqualTo(ratings));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved ratings for user")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task GetRatingsForUserAsync_WhenErrorOccurs_LogsErrorAndReturnsFailureResponse()
        {
            // Arrange
            var userId = "user123";
            var cancellationToken = new CancellationToken();

            _ratingRepositoryMock.Setup(x => x.GetRatingsForUserAsync(userId, cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _ratingService.GetRatingsForUserAsync(userId, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Title, Is.EqualTo("Database error"));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while retrieving ratings")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
