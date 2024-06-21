using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Movies.Application.Models
{
    public partial class Movie
    {
        public required Guid Id { get; init; }
        public required string Title { get; set; }
        public float? Rating { get; set; }
        public int? UserRating { get; set; }
        public string Slug => GenerateSlug();
        public required int YearOfRelease { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public IEnumerable<OmdbRating> Ratings { get; set; } = Enumerable.Empty<OmdbRating>();
        public string Metascore { get; set; }

        [JsonPropertyName("imdbRating")]
        public string ImdbRating { get; set; }

        [JsonPropertyName("imdbVotes")]
        public string ImdbVotes { get; set; }
        public string TotalSeasons { get; set; }
        public List<string> Genres { get; set; } = new();

        private string GenerateSlug()
        {
            var sluggedTitle = SlugRegex().Replace(Title, string.Empty)
                .ToLower().Replace(" ", "-");
            return $"{sluggedTitle}-{YearOfRelease}";
        }

        [GeneratedRegex("[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 5)]
        private static partial Regex SlugRegex();

    }
}
