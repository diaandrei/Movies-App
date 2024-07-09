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
        public IEnumerable<Cast> Cast { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<ExternalRating> ExternalRatings { get; set; }
        public virtual IEnumerable<OmdbRating> OmdbRatings { get; set; }
        public virtual IEnumerable<MovieRating> MovieRatings { get; set; }
        public decimal? Rating { get; set; }
        public decimal? UserRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
