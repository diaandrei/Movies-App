namespace Movies.Contracts.Responses
{
    public class MovieResponse
    {
        public required Guid Id { get; init; }
        public required string Title { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public required string YearOfRelease { get; set; }
        public string Rated { get; set; }
        public string Plot { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public string? TotalSeasons { get; set; }
        public bool IsActive { get; set; }
        public List<CastResponse> Cast { get; set; }
        public List<GenreResponse> Genres { get; set; }
        public List<ExternalRatingResponse> ExternalRatings { get; set; }
        public List<OmdbRatingResponse> OmdbRatings { get; set; }
        public List<MovieRatingResponse> MovieRatings { get; set; }
        public List<UserWatchlistResponse> UserWatchlists { get; set; } = new List<UserWatchlistResponse>();
        public decimal? Rating { get; set; }
        public bool IsUserRated { get; set; }
        public Guid UserWatchlistId { get; set; }
        public bool IsMovieWatchlist { get; set; }
        public decimal? UserRating { get; set; }
        public string FirstAddedToWatchlistAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
