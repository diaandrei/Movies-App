﻿namespace Movies.Contracts.Responses
{
    public class MovieResponseDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid UserWatchlistId { get; set; }
        public string Title { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string YearOfRelease { get; set; }
        public string Rated { get; set; }
        public string Plot { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public string? TotalSeasons { get; set; }
        public bool IsActive { get; set; }
        public decimal? Rating { get; set; }
        public decimal? UserRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsMovieWatchlist { get; set; }
        public string FirstAddedToWatchlistAt { get; set; }
        public List<CastResponseDto> Cast { get; set; }
        public List<GenreResponseDto> Genres { get; set; }
        public List<ExternalRatingResponseDto> ExternalRatings { get; set; }
        public List<OmdbRatingResponseDto> OmdbRatings { get; set; }
        public List<MovieRatingResponseDto> MovieRatings { get; set; }
    }
}
