using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Movies.Application.Models;

namespace Movies.Application.Database
{
    public class MoviesDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;

        public MoviesDbContext(DbContextOptions<MoviesDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<TopMovie> TopMovies { get; set; }
        public DbSet<MovieRating> MovieRatings { get; set; }
        public DbSet<OmdbRating> OmdbRatings { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<UserWatchlist> UserWatchlists { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Cast> Casts { get; set; }
        public DbSet<ExternalRating> ExternalRatings { get; set; }
        public DbSet<MovieGenres> MovieGenres { get; set; }
        public DbSet<MovieCast> MovieCast { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("Database");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Database connection string is not configured.");
                }

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}