namespace Movies.Application.Models
{
    public class MovieCast
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public Guid CastId { get; set; }
        public Movie Movie { get; set; }
        public Cast Cast { get; set; }
    }
}
