namespace Movies.Application.Models
{
    public class Movie
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required int YearOfRelease { get; set; }
        public List<string> Genres { get; init; } = new();
    }
}
