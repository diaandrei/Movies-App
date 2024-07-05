namespace Movies.Application.Models
{
    public class ExternalRating
    {
        public Guid Id { get; set; }
        public string Rating { get; set; }
        public string Source { get; set; }
    }
}
