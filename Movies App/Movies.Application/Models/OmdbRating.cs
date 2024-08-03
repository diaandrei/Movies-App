using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Movies.Application.Models
{
    public class OmdbRating
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }
        [JsonIgnore]
        public virtual Movie Movie { get; set; }
    }
}
