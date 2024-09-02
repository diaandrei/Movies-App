using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Identity;

namespace Movies.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<MoviesDbContext>(
                options => options.UseSqlServer(connectionString));
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<IUserWatchlistRepository, UserWatchlistRepository>();
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IUserWatchlistService, UserWatchlistService>();
            services.AddScoped<IOmdbService, OmdbService>();
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<ICastRepository, CastRepository>();

            services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Scoped);

            return services;
        }
    }
}