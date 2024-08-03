namespace Movies.Contracts.Responses
{
    public class ExternalRatingResponseDto
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string Rating { get; set; }
    }
}
