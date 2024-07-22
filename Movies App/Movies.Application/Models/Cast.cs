namespace Movies.Application.Models
{
    public class Cast
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
    }
}
