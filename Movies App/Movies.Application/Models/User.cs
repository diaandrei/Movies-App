using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
