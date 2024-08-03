namespace Movies.Application.DataTransferObjects
{
    public class ExternalRatingDto
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string Rating { get; set; }
    }
}
