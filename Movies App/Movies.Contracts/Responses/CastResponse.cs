namespace Movies.Contracts.Responses
{
    public class CastResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
