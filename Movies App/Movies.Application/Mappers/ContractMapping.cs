using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Application
{
    public static class ContractMapping
    {
        public static Genre ToEntity(this GenreRequest request)
        {
            return new Genre
            {
                Id = request.Id,
                Name = request.Name,
                CreatedAt = request.CreatedAt,
            };
        }

        public static GenreResponse ToResponse(this Genre entity)
        {
            return new GenreResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt
            };
        }

        public static Movie MapToMovie(this CreateMovieRequest request)
        {
            return new Movie
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                YearOfRelease = request.YearOfRelease
            };
        }

        public static Movie PopulateValuesFromOmdb(this Movie movie, OmdbResponse omdb)
        {
            movie.Title = omdb.Title;
            movie.YearOfRelease = omdb.Year;
            movie.Rated = omdb.Rated;
            movie.Released = omdb.Released;
            movie.Runtime = omdb.Runtime;
            movie.OmdbRatings = omdb.Ratings.Select(x => new OmdbRating
            {
                Source = x.Source,
                Value = x.Value
            });

            movie.Plot = omdb.Plot;
            movie.Awards = omdb.Awards;
            movie.Poster = omdb.Poster;
            movie.TotalSeasons = omdb.TotalSeasons;

            return movie;
        }

        public static MovieResponse MapToResponse(this Movie movie)
        {
            return new MovieResponse
            {
                Id = movie.Id,
                Title = movie.Title,
                Released = movie.Released,
                Runtime = movie.Runtime,
                YearOfRelease = movie.YearOfRelease,
                Rated = movie.Rated,
                Plot = movie.Plot,
                Awards = movie.Awards,
                Poster = movie.Poster,
                TotalSeasons = movie.TotalSeasons,
                IsActive = movie.IsActive,
                Cast = movie.Cast.Select(x => new CastResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Role = x.Role
                }),
                Genres = movie.Genres.Select(c => new GenreResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                }),
                ExternalRatings = movie.ExternalRatings.Select(x => new ExternalRatingResponse
                {
                    Source = x.Source,
                    Rating = x.Rating
                }),
                OmdbRatings = movie.OmdbRatings.Select(x => new OmdbRatingResponse
                {
                    Source = x.Source,
                    Value = x.Value
                }),
                MovieRatings = movie.MovieRatings.Select(x => new MovieRatingResponse
                {
                    Rating = x.Rating,
                    MovieId = x.MovieId,
                }),
                Rating = movie.Rating,
                UserRating = movie.UserRating,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt
            };
        }

        public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies,
            int page, int pageSize, int totalCount)
        {
            return new MoviesResponse
            {
                Items = movies.Select(MapToResponse),
                Page = page,
                PageSize = pageSize,
                Total = totalCount
            };
        }

        public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
        {
            return new Movie
            {
                Id = id,
                Title = request.Title,
                Released = request.Released,
                Runtime = request.Runtime,
                YearOfRelease = request.YearOfRelease,
                Rated = request.Rated,
                Plot = request.Plot,
                Awards = request.Awards,
                Poster = request.Poster,
                TotalSeasons = request.TotalSeasons,
                Cast = request.Cast.Select(x => new Cast
                {
                    Id = x.Id,
                    Name = x.Name,
                    Role = x.Role
                }),
                Genres = request.Genres.Select(x => new Genre
                {
                    Id = x.Id,
                    Name = x.Name
                }),
                ExternalRatings = request.ExternalRatings.Select(x => new ExternalRating
                {
                    Source = x.Source,
                    Rating = x.Rating
                }),
                OmdbRatings = request.OmdbRatings.Select(x => new OmdbRating
                {
                    Source = x.Source,
                    Value = x.Value
                }),
                MovieRatings = request.MovieRatings.Select(x => new MovieRating
                {
                    Rating = x.Rating,
                    MovieId = x.MovieId
                }),
                Rating = request.Rating,
                UserRating = request.UserRating,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };
        }
        public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating> ratings)
        {
            return ratings.Select(x => new MovieRatingResponse
            {
                Rating = (int)x.Rating,
                MovieId = x.MovieId,
            });
        }
        public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
        {
            return new GetAllMoviesOptions
            {
                Title = request.Title,
                YearOfRelease = request.Year,
                SortOrder = request.SortBy == null ? SortOrder.Unsorted :
                request.SortBy.StartsWith('-') ? SortOrder.Descending :
                SortOrder.Ascending,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
        public static GetAllMoviesOptions WithUserId(this GetAllMoviesOptions options, Guid? userId)
        {
            options.UserId = userId;
            return options;
        }
    }
}
