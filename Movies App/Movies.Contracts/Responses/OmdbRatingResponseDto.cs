namespace Movies.Contracts.Responses
{
    public class OmdbRatingResponseDto
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }
    }
}
