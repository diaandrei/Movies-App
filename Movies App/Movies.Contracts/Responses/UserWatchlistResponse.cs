namespace Movies.Contracts.Responses
{
    public class UserWatchlistResponse
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
