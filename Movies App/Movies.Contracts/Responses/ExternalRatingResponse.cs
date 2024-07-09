namespace Movies.Contracts.Responses
{
    public class ExternalRatingResponse
    {
        public Guid Id { get; set; }
        public string Rating { get; set; }
        public string? Source { get; set; }
    }
}
