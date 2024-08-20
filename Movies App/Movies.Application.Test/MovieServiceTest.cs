using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Movies.Application.Database;
using Movies.Application.Mappers;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Application.Test
{
    public class MovieServiceTest
    {
        private MoviesDbContext _context;
        private IRatingRepository _ratingRepository;
        private IMovieRepository _movieRepository;
        private IMovieService _movieService;
        private IValidator<Movie> _movieValidator;
        private IValidator<GetAllMoviesOptions> _optionsValidator;
        private ILogger _logger;
        private IOmdbService _omdbService;
        private IMapper _autoMapper;


        [SetUp]
        public void Setup()
        {
            var dbContextOptions = new DbContextOptionsBuilder<MoviesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x.Value).Returns("Test");


            mockConfiguration.Setup(x => x.GetSection("IsTest"))
                         .Returns(mockConfigurationSection.Object);

            _context = new MoviesDbContext(dbContextOptions, mockConfiguration.Object);

            _movieRepository = new MovieRepository(_context);
            _ratingRepository = new RatingRepository(_context);

            var validator = new Mock<IValidator<Movie>>();
            validator.Setup(c => c.Validate(It.IsAny<Movie>())).Returns(new ValidationResult { Errors = new() });

            var optionsValidator = new Mock<IValidator<GetAllMoviesOptions>>();
            optionsValidator.Setup(c => c.Validate(It.IsAny<GetAllMoviesOptions>())).Returns(new ValidationResult { Errors = new() });

            var expectedResponse = new ResponseModel<OmdbResponse>
            {
                Content = new OmdbResponse { Title = "Test Movie" },
                Title = "Title successfully retrieved from Omdb.",
                Success = true
            };

            var omdbServiceMock = new Mock<IOmdbService>();

            omdbServiceMock
                .Setup(c => c.GetMovieAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var logger = new Mock<ILogger<MovieService>>();

            var configMap = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
                cfg.SourceMemberNamingConvention = LowerUnderscoreNamingConvention.Instance;
                cfg.DestinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
                cfg.AllowNullCollections = true;
            });
            var mapper = configMap.CreateMapper();


            _movieService = new MovieService(
                _movieRepository,
                validator.Object,
                _ratingRepository,
                optionsValidator.Object,
                omdbServiceMock.Object,
                logger.Object,
                mapper
            );
        }

        [TearDown]
        public void TearDown()
        {
            //_context.Database.EnsureDeleted();
        }

        [Test]
        public async Task AddMovieAsync_ShouldReturnResponseModel()
        {
            var movie = new Movie
            {
                Title = "Test Movie",
                YearOfRelease = "2021"
            };

            var response = await _movieService.CreateAsync(movie, CancellationToken.None);

            Assert.That(response.Success);
        }

    }
}