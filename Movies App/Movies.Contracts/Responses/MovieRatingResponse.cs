namespace Movies.Contracts.Responses
{
    public class MovieRatingResponse
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public decimal Rating { get; set; }
        public string UserId { get; set; }
        public bool IsUserRated { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
