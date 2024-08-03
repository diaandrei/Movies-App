namespace Movies.Application.DataTransferObjects
{
    public class UserWatchlistDto
    {
        public Guid UserWatchlistId { get; set; }
        public IEnumerable<MovieDto> Movie { get; set; }
    }
}
