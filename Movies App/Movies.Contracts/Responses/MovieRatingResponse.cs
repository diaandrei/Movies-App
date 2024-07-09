namespace Movies.Contracts.Responses
{
    public class MovieRatingResponse
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public decimal Rating { get; set; }
        public Guid UserId { get; set; }
        public virtual MovieResponse Movie { get; set; }
    }
}
