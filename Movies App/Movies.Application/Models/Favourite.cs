using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Models
{
    public class Favourite
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public virtual Movie Movie { get; set; }
    }
}