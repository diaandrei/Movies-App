using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.Application.DataTransferObjects;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application.Test
{
    [TestFixture]
    public class UserWatchlistServiceTests
    {
        private Mock<IUserWatchlistRepository> _userWatchlistRepositoryMock;
        private Mock<ILogger<UserWatchlistService>> _loggerMock;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private UserWatchlistService _userWatchlistService;

        [SetUp]
        public void Setup()
        {
            _userWatchlistRepositoryMock = new Mock<IUserWatchlistRepository>();
            _loggerMock = new Mock<ILogger<UserWatchlistService>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null!, null!, null!, null!, null!, null!, null!, null!);

            _userWatchlistService = new UserWatchlistService(_userWatchlistRepositoryMock.Object, _loggerMock.Object, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _userWatchlistRepositoryMock = null!;
            _loggerMock = null!;
            _userManagerMock = null!;
        }

        [Test]
        public async Task DeleteByIdAsync_WhenMovieExists_DeletesSuccessfully()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            _userWatchlistRepositoryMock.Setup(repo => repo.DeleteByIdAsync(movieId, cancellationToken))
                .ReturnsAsync(true);

            // Act
            var result = await _userWatchlistService.DeleteByIdAsync(movieId, cancellationToken);

            // Assert
            Assert.That(result, Is.True);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully deleted title with ID")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public void DeleteByIdAsync_WhenMovieDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            _userWatchlistRepositoryMock.Setup(repo => repo.DeleteByIdAsync(movieId, cancellationToken))
                .ReturnsAsync(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _userWatchlistService.DeleteByIdAsync(movieId, cancellationToken));
            Assert.That(ex?.Message, Is.EqualTo($"Movie with ID '{movieId}' does not exist."));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Title with ID: {movieId} does not exist.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task GetAllAsync_WhenMoviesExist_ReturnsMoviesSuccessfully()
        {
            // Arrange
            var userId = "user-id";
            var cancellationToken = new CancellationToken();
            var movies = new List<MovieDto>
            {
                new MovieDto { Id = Guid.NewGuid(), Title = "Movie 1" },
                new MovieDto { Id = Guid.NewGuid(), Title = "Movie 2" }
            };

            _userWatchlistRepositoryMock.Setup(repo => repo.GetAllAsync(false, userId, cancellationToken))
                .ReturnsAsync(movies);

            // Act
            var result = await _userWatchlistService.GetAllAsync(userId, false, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(movies));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved all titles")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public void UpdateAsync_WhenMovieDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var movie = new UserWatchlist { Id = Guid.NewGuid() };
            var cancellationToken = new CancellationToken();

            _userWatchlistRepositoryMock.Setup(repo => repo.GetByIdAsync(movie.Id, false, null, cancellationToken))
                .ReturnsAsync((UserWatchlist?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _userWatchlistService.UpdateAsync(movie, null, cancellationToken));
            Assert.That(ex?.Message, Is.EqualTo($"Title with ID '{movie.Id}' does not exist."));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Title with ID: {movie.Id} does not exist.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public async Task AddMovieInWatchlistAsync_WhenSuccess_ReturnsSuccessfulResponse()
        {
            // Arrange
            var userWatchlist = new UserWatchlist { UserId = "user-id", MovieId = Guid.NewGuid() };
            var cancellationToken = new CancellationToken();

            _userWatchlistRepositoryMock.Setup(repo => repo.AddMovieInWatchlistAsync(userWatchlist, cancellationToken))
                .ReturnsAsync(userWatchlist);

            _userWatchlistRepositoryMock.Setup(repo => repo.CountUserWatchlistAsync(userWatchlist.UserId, cancellationToken))
                .ReturnsAsync(true);

            _userManagerMock.Setup(manager => manager.FindByIdAsync(userWatchlist.UserId))
                .ReturnsAsync(new ApplicationUser { Id = userWatchlist.UserId });

            _userManagerMock.Setup(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var response = await _userWatchlistService.AddMovieInWatchlistAsync(userWatchlist);

            // Assert
            Assert.That(response.Success, Is.True);
            Assert.That(response.Title, Is.EqualTo("Successfully added the title to your watchlist"));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully added title to watchlist")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Test]
        public void AddMovieInWatchlistAsync_WhenExceptionThrown_LogsErrorAndThrows()
        {
            // Arrange
            var userWatchlist = new UserWatchlist { UserId = "user-id", MovieId = Guid.NewGuid() };
            var cancellationToken = new CancellationToken();

            _userWatchlistRepositoryMock.Setup(repo => repo.AddMovieInWatchlistAsync(userWatchlist, cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _userWatchlistService.AddMovieInWatchlistAsync(userWatchlist));
            Assert.That(ex?.Message, Is.EqualTo("Database error"));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while adding title to watchlist")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
