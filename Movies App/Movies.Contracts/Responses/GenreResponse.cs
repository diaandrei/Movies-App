namespace Movies.Contracts.Responses
{
    public class GenreResponse
    {
        public class Genre
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string Name { get; set; }
        }
    }
}
