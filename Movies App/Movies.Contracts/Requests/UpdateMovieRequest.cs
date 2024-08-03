using Movies.Contracts.Responses;

namespace Movies.Contracts.Requests
{
    public class UpdateMovieRequest
    {
        public required Guid Id { get; init; }
        public string Plot { get; set; }

        //todo: Review those later 
        //public required string Title { get; set; }
        //public string Released { get; set; }
        //public string Runtime { get; set; }
        //public required string YearOfRelease { get; set; }
        //public string Rated { get; set; }
        //public string Awards { get; set; }
        //public string Poster { get; set; }
        //public string? TotalSeasons { get; set; }
        //public bool IsActive { get; set; }
        //public IEnumerable<CastResponse> Cast { get; set; }
        //public IEnumerable<GenreResponse> Genres { get; init; } = Enumerable.Empty<GenreResponse>();
        //public IEnumerable<ExternalRatingResponse> ExternalRatings { get; set; }
        //public IEnumerable<OmdbRatingResponse> OmdbRatings { get; set; } = Enumerable.Empty<OmdbRatingResponse>();
        //public IEnumerable<MovieRatingResponse> MovieRatings { get; set; }
        //public decimal? Rating { get; set; }
        //public decimal? UserRating { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public DateTime? UpdatedAt { get; set; }

    }
}
