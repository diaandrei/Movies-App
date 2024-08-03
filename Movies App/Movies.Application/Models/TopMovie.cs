using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Models
{
    public class TopMovie
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public Movie Movie { get; set; }
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
