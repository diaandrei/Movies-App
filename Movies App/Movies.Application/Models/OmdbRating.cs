using System.ComponentModel.DataAnnotations;

namespace Movies.Application.Models
{
    public class OmdbRating
    {
        [Key]
        public Guid Id { get; set; }
        public int MovieId { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }

        public virtual Movie Movie { get; set; }
    }
}
