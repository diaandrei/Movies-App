using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Models
{
    public class MovieGenres
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public Guid GenreId { get; set; }
        public Movie Movie { get; set; }
        public Genre Genre { get; set; }
    }
}
