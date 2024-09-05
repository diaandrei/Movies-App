using Movies.Contracts.Responses;

namespace Movies.Contracts.Requests
{
    public class UpdateMovieRequest
    {
        public required Guid Id { get; init; }
        public string Runtime { get; set; }
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
        public List<CastResponse> Cast { get; set; } = new List<CastResponse>();
        public List<GenreResponse> Genres { get; init; } = new List<GenreResponse>();
        public List<ExternalRatingResponse> ExternalRatings { get; set; } = new List<ExternalRatingResponse>();
        public List<OmdbRatingResponse> OmdbRatings { get; set; } = new List<OmdbRatingResponse>();
        public List<MovieRatingResponse> MovieRatings { get; set; } = new List<MovieRatingResponse>();
    }
}
