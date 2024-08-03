using Movies.Application.DataTransferObjects;
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
            if (omdb == null)
            {
                throw new ArgumentNullException(nameof(omdb), "OMDb response cannot be null.");
            }

            movie.Title = omdb.Title;
            movie.Released = omdb.Released;
            movie.Runtime = omdb.Runtime;
            movie.YearOfRelease = omdb.Year;
            movie.Rated = omdb.Rated;
            movie.Plot = omdb.Plot;
            movie.Awards = omdb.Awards;
            movie.Poster = omdb.Poster;
            movie.TotalSeasons = omdb.TotalSeasons;
            movie.IsActive = true;
            movie.CreatedAt = DateTime.UtcNow;
            movie.UpdatedAt = DateTime.UtcNow;

            return movie;
        }

        public static List<Genre> PopulateGenresFromOmdb(this Movie movie, string genresCommaSeparated)
        {
            List<Genre> Genres = new List<Genre>();

            if (string.IsNullOrWhiteSpace(genresCommaSeparated))
            {
                return Genres;
            }

            var genreNames = genresCommaSeparated.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var genreName in genreNames)
            {
                var trimmedGenreName = genreName.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedGenreName))
                {
                    var genre = new Genre
                    {
                        Id = Guid.NewGuid(),
                        Name = trimmedGenreName,
                    };

                    Genres.Add(genre);
                }
            }
            return Genres;
        }

        public static List<Cast> PopulateCastFromOmdb(this Movie movie, string castCommaSeparated)
        {
            List<Cast> casts = new List<Cast>();

            if (string.IsNullOrWhiteSpace(castCommaSeparated))
            {
                return null;
            }

            var genreNames = castCommaSeparated.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var genreName in genreNames)
            {
                var trimmedGenreName = genreName.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedGenreName))
                {
                    var genre = new Cast
                    {
                        Id = Guid.NewGuid(),
                        Name = trimmedGenreName,
                        Role = "i",
                    };
                    casts.Add(genre);
                }
            }

            return casts;
        }
        public static MovieResponse MapToResponse(this Movie movie, string userId = null)
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
                }).ToList(),

                Genres = movie.Genres.Select(c => new GenreResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                }).ToList(),

                ExternalRatings = movie.ExternalRatings.Select(x => new ExternalRatingResponse
                {
                    Source = x.Source,
                    Rating = x.Rating
                }).ToList(),

                OmdbRatings = movie.OmdbRatings.Select(x => new OmdbRatingResponse
                {
                    Source = x.Source,
                    Value = x.Value
                }).ToList(),

                MovieRatings = movie.MovieRatings.Select(x => new MovieRatingResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Rating = x.Rating,
                    IsUserRated = x.IsUserRated

                }).ToList(),

                UserWatchlists = movie.UserWatchlists.Select(x => new UserWatchlistResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    MovieId = x.MovieId,
                    CreatedAt = x.CreatedAt

                }).ToList(),

                Rating = movie.Rating,
                IsUserRated = movie.MovieRatings.Count > 0 ? true : false,
                IsMovieWatchlist = movie.UserWatchlists.Count > 0 ? true : false,
                UserRating = movie.UserRating,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt,
                FirstAddedToWatchlistAt = FirstAddedToWatchlistAgo(movie.ApplicationUser?.FirstAddedToWatchlistAt),
            };
        }

        private static string FirstAddedToWatchlistAgo(DateTime? datetime)
        {
            if (!datetime.HasValue)
            {
                return string.Empty;
            }

            var timeSpan = DateTime.UtcNow - datetime.Value;
            var daysAgo = timeSpan.Days;

            return daysAgo == 0 ? "Today" : $"{daysAgo} days ago";
        }

        public static MoviesResponseDto MapToResponse(this IEnumerable<MovieDto> movies,
            int page, int pageSize, int totalCount)
        {
            return new MoviesResponseDto
            {
                Items = movies.Select(MapToResponseDto),
                Page = page,
                PageSize = pageSize,
                Total = totalCount

            };

        }

        public static MovieResponseDto MapToResponseDto(this MovieDto movieDTO)
        {
            return new MovieResponseDto
            {
                Id = movieDTO.Id,
                UserWatchlistId = movieDTO.UserWatchlistId,
                Title = movieDTO.Title,
                Released = movieDTO.Released,
                Runtime = movieDTO.Runtime,
                YearOfRelease = movieDTO.YearOfRelease,
                Rated = movieDTO.Rated,
                Plot = movieDTO.Plot,
                Awards = movieDTO.Awards,
                Poster = movieDTO.Poster,
                TotalSeasons = movieDTO.TotalSeasons,
                IsActive = movieDTO.IsActive,
                Rating = movieDTO.Rating,
                UserRating = movieDTO.UserRating,
                CreatedAt = movieDTO.CreatedAt,
                UpdatedAt = movieDTO.UpdatedAt,
                Cast = movieDTO.Cast.Select(c => new CastResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Role = c.Role
                }).ToList(),

                Genres = movieDTO.Genres.Select(g => new GenreResponseDto
                {
                    Id = g.Id,
                    Name = g.Name
                }).ToList(),

                ExternalRatings = movieDTO.ExternalRatings.Select(er => new ExternalRatingResponseDto
                {
                    Id = er.Id,
                    Source = er.Source,
                }).ToList(),

                OmdbRatings = movieDTO.OmdbRatings.Select(om => new OmdbRatingResponseDto
                {
                    Id = om.Id,
                    Source = om.Source,
                    Value = om.Value
                }).ToList(),

                MovieRatings = movieDTO.MovieRatings.Select(mr => new MovieRatingResponseDto
                {
                    Id = mr.Id,
                    Rating = mr.Rating,
                }).ToList(),
                FirstAddedToWatchlistAt = FirstAddedToWatchlistAgo(movieDTO.ApplicationUser?.FirstAddedToWatchlistAt),
                UserId = movieDTO.ApplicationUser.Id,
                IsMovieWatchlist = true
            };
        }

        public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies,
            int page, int pageSize, int totalCount, string userId = null)
        {
            return new MoviesResponse
            {
                Items = movies.Select(movie => movie.MapToResponse(userId)),
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
                Plot = request.Plot,
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

        public static UserWatchlist MapToWatchlist(Guid movieId, Guid userId)
        {
            return new UserWatchlist
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                MovieId = movieId,
                UserId = userId.ToString()
            };
        }
        public static UserWatchlist DeleteRequestMapToWatchlist(Guid watchlistId, Guid userId)
        {
            return new UserWatchlist
            {
                Id = watchlistId,
                UserId = userId.ToString(),
                IsActive = false,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        public static MovieRating MapToRatingRequest(Guid ratingId, Guid movieId, decimal movieRating, string userId)
        {
            return new MovieRating
            {
                Id = ratingId == Guid.Empty ? Guid.NewGuid() : ratingId,
                MovieId = movieId,
                UserId = userId,
                Rating = movieRating,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static List<TopMovie> MapTopMovieRequest(List<Guid> movieIds, string userId)
        {
            return movieIds.Select(movieId => new TopMovie
            {
                Id = Guid.NewGuid(),
                MovieId = movieId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            }).ToList();
        }
    }
}