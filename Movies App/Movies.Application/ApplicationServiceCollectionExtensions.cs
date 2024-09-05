using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Movies.Application;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string connectionString, string omdbApiKey)
    {
        services.AddDbContext<MoviesDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IUserWatchlistRepository, UserWatchlistRepository>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IUserWatchlistService, UserWatchlistService>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<ICastRepository, CastRepository>();

        services.AddScoped<IOmdbService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<OmdbService>>();
            var client = provider.GetRequiredService<HttpClient>();
            return new OmdbService(logger, client, omdbApiKey);
        });

        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Scoped);

        return services;
    }
}