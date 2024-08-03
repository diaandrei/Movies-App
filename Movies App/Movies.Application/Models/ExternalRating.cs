namespace Movies.Application.Models
{
    public class ExternalRating
    {
        public Guid Id { get; set; }
        public string Rating { get; set; }
        public string Source { get; set; }
        public Guid MovieId { get; set; }
        public virtual Movie Movie { get; set; }
    }
}
