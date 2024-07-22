using Microsoft.EntityFrameworkCore;
using Movies.Application.Models;

namespace Movies.Application.Database
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext() { }
        public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieRating> MovieRatings { get; set; }
        public DbSet<OmdbRating> OmdbRatings { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Cast> Casts { get; set; }
        public DbSet<ExternalRating> ExternalRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=tcp:dev-data-server-andrei.database.windows.net,1433;Initial Catalog=movies;Persist Security Info=False;User ID=admin-sa;Password=6x2134013A;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
}
