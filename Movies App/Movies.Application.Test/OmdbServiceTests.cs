using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Application.Test
{
    [TestFixture]
    public class OmdbServiceTests
    {
        private Mock<ILogger<OmdbService>> _loggerMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private OmdbService _omdbService;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<OmdbService>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            string apiKey = "test-api-key";
            _omdbService = new OmdbService(_loggerMock.Object, _httpClient, apiKey);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task GetMovieAsync_WhenMovieExists_ReturnsSuccessfulResponse()
        {
            // Arrange
            var title = "Inception";
            var year = "2010";
            var cancellationToken = new CancellationToken();

            var omdbResponse = new OmdbResponse
            {
                Title = "Inception",
                Year = "2010",
            };

            var responseContent = JsonSerializer.Serialize(omdbResponse);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var response = await _omdbService.GetMovieAsync(title, year, cancellationToken);

            // Assert
            Assert.That(response, Is.InstanceOf<ResponseModel<OmdbResponse>>());
            Assert.That(response.Success, Is.True);
            Assert.That(response.Content.Title, Is.EqualTo(title));
        }

        [Test]
        public async Task GetMovieAsync_WhenMovieDoesNotExist_ReturnsFailureResponse()
        {
            // Arrange
            var title = "NonExistentMovie";
            var year = "1900";
            var cancellationToken = new CancellationToken();

            var omdbResponse = new OmdbResponse
            {
                Title = null!
            };

            var responseContent = JsonSerializer.Serialize(omdbResponse);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var response = await _omdbService.GetMovieAsync(title, year, cancellationToken);

            // Assert
            Assert.That(response.Success, Is.False);
            Assert.That(response.Title, Is.EqualTo("The title does not exist."));
        }

        [Test]
        public async Task GetMovieAsync_WhenApiCallFails_LogsErrorAndReturnsNull()
        {
            // Arrange
            var title = "Inception";
            var year = "2010";
            var cancellationToken = new CancellationToken();

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var response = await _omdbService.GetMovieAsync(title, year, cancellationToken);

            // Assert
            Assert.That(response, Is.Null);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get title from OMDB API")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

    }
}
