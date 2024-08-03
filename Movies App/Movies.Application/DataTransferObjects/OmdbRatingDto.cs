namespace Movies.Application.DataTransferObjects
{
    public class OmdbRatingDto
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }
    }
}
