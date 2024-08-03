namespace Movies.Contracts.Requests
{
    public class RateMovieRequest
    {
        public decimal Rating { get; set; }
        public Guid MovieId { get; set; }
        public Guid RatingId { get; set; }
    }
}
