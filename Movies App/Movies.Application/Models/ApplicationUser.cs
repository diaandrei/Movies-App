using Microsoft.AspNetCore.Identity;

namespace Movies.Application.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsTrustedMember { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? FirstAddedToWatchlistAt { get; set; }
        public ICollection<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();
    }
}
