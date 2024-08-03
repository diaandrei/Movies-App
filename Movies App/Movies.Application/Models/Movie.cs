using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Models
{
    public class Movie
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string YearOfRelease { get; set; }
        public string Rated { get; set; }
        public string Plot { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public string? TotalSeasons { get; set; }
        public bool IsActive { get; set; }
        public decimal? Rating { get; set; }
        public decimal? UserRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public List<Cast> Cast { get; set; } = new List<Cast>();
        public List<UserWatchlist> UserWatchlists { get; set; } = new List<UserWatchlist>();

        public List<ExternalRating> ExternalRatings { get; set; } = new List<ExternalRating>();
        public List<OmdbRating> OmdbRatings { get; set; } = new List<OmdbRating>();
        public List<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();
    }
}