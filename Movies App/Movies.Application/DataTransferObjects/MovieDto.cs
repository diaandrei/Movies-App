using Movies.Application.Models;
using System.Text.Json.Serialization;

namespace Movies.Application.DataTransferObjects
{
    public class MovieDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
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
        [JsonIgnore]
        public ApplicationUser ApplicationUser { get; set; }
        public Guid UserWatchlistId { get; set; }
        public DateTime? FirstAddedToWatchlistAt { get; set; }
        public List<CastDto> Cast { get; set; }
        public List<GenreDto> Genres { get; set; }
        public List<ExternalRatingDto> ExternalRatings { get; set; }
        public List<OmdbRatingDto> OmdbRatings { get; set; }
        public List<MovieRatingDto> MovieRatings { get; set; }
    }
}
