namespace Movies.Contracts.Requests
{
    public class CreateMovieRequest
    {
        public required string Title { get; set; }
        public required int YearOfRelease { get; set; }
    }
}
