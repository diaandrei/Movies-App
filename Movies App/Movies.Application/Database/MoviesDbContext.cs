using Microsoft.AspNetCore.Identity;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Name = "Admin",
                NormalizedName = "Admin",
                Id = "1",
                ConcurrencyStamp = "1"
            });

            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Name = "User",
                NormalizedName = "User",
                Id = "2",
                ConcurrencyStamp = "2"
            });
            var adminUserId = Guid.NewGuid().ToString();

            var appUser = new ApplicationUser
            {
                Id = adminUserId,
                Email = "aamoviesapp@gmail.com",
                EmailConfirmed = true,
                FirstName = "Movies",
                LastName = "Admin",
                UserName = "aamoviesapp@gmail.com",
                IsTrustedMember = true,
                IsAdmin = true,
                NormalizedUserName = "AAMOVIESAPP@GMAIL.COM",
                NormalizedEmail = "AAMOVIESAPP@GMAIL.COM"
            };

            PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();
            appUser.PasswordHash = ph.HashPassword(appUser, "134013Aa!");

            builder.Entity<ApplicationUser>().HasData(appUser);

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = "1",
                UserId = adminUserId
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Database");
        }
    }
}
