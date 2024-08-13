using Movies.Contracts.Responses;

namespace Movies.Contracts.Requests
{
    public class UpdateMovieRequest
    {
        public required Guid Id { get; init; }
        public string Plot { get; set; }
        public List<OmdbRatingResponse> OmdbRatings { get; set; } = new List<OmdbRatingResponse>();
        public List<CastResponse> Cast { get; set; } = new List<CastResponse>();
    }
}
