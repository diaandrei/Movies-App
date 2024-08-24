using System.Text.Json.Serialization;

namespace Movies.Contracts.Responses
{
    public class OmdbResponse
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public IEnumerable<OmdbRatingResponse> Ratings { get; set; } = Enumerable.Empty<OmdbRatingResponse>();
        public string Metascore { get; set; }

        [JsonPropertyName("imdbRating")]
        public string ImdbRating { get; set; }

        [JsonPropertyName("imdbVotes")]
        public string ImdbVotes { get; set; }

        [JsonPropertyName("imdbID")]
        public string ImdbID { get; set; }

        public string Type { get; set; }
        public string TotalSeasons { get; set; }
        public string Response { get; set; }
    }

    public class OmdbRatingResponse
    {
        public Guid Id { get; set; }
        public string? Source { get; set; }
        public string Value { get; set; }
    }
}
