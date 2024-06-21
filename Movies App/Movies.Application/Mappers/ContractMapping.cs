using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Application
{
    public static class ContractMapping
    {
        public static Movie MapToMovie(this CreateMovieRequest request)
        {
            return new Movie
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                YearOfRelease = request.YearOfRelease,
                Genres = request.Genres.ToList()
            };
        }

        public static Movie PopulateValuesFromOmdb(this Movie movie, OmdbResponse omdb)
        {
            movie.Title = omdb.Title;
            movie.YearOfRelease = int.Parse(omdb.Year);
            movie.Genres = omdb.Genre.Split(',').ToList();
            movie.Rated = omdb.Rated;
            movie.Released = omdb.Released;
            movie.Runtime = omdb.Runtime;
            movie.Director = omdb.Director;
            movie.Writer = omdb.Writer;
            movie.Actors = omdb.Actors;
            movie.Plot = omdb.Plot;
            movie.Awards = omdb.Awards;
            movie.Poster = omdb.Poster;
            movie.Ratings = omdb.Ratings.Select(x => new OmdbRating
            {
                Source = x.Source,
                Value = x.Value
            }).ToList();
            movie.Metascore = omdb.Metascore;
            movie.ImdbRating = omdb.ImdbRating;
            movie.ImdbVotes = omdb.ImdbVotes;
            movie.TotalSeasons = omdb.TotalSeasons;

            return movie;
        }

        public static MovieResponse MapToResponse(this Movie movie)
        {
            return new MovieResponse
            {
                Id = movie.Id,
                Title = movie.Title,
                Slug = movie.Slug,
                Rating = movie.Rating,
                UserRating = movie.UserRating,
                YearOfRelease = movie.YearOfRelease,
                Genres = movie.Genres,
                Rated = movie.Rated,
                Released = movie.Released,
                Runtime = movie.Runtime,
                Director = movie.Director,
                Writer = movie.Writer,
                Actors = movie.Actors,
                Plot = movie.Plot,
                Awards = movie.Awards,
                Poster = movie.Poster,
                Ratings = movie.Ratings.Select(x => new OmdbRatingResponse
                {
                    Source = x.Source,
                    Value = x.Value
                }),
                Metascore = movie.Metascore,
                ImdbRating = movie.ImdbRating,
                ImdbVotes = movie.ImdbVotes,
                TotalSeasons = movie.TotalSeasons,
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
                Rating = request.Rating,
                UserRating = request.UserRating,
                YearOfRelease = request.YearOfRelease,
                Genres = request.Genres,
                Rated = request.Rated,
                Released = request.Released,
                Runtime = request.Runtime,
                Director = request.Director,
                Writer = request.Writer,
                Actors = request.Actors,
                Plot = request.Plot,
                Awards = request.Awards,
                Poster = request.Poster,
                Ratings = request.Ratings.Select(x => new OmdbRating
                {
                    Source = x.Source,
                    Value = x.Value
                }),
                Metascore = request.Metascore,
                ImdbRating = request.ImdbRating,
                ImdbVotes = request.ImdbVotes,
                TotalSeasons = request.TotalSeasons,
            };
        }
        public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating> ratings)
        {
            return ratings.Select(x => new MovieRatingResponse
            {
                Rating = x.Rating,
                Slug = x.Slug,
                MovieId = x.MovieId
            });
        }
        public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
        {
            return new GetAllMoviesOptions
            {
                Title = request.Title,
                YearOfRelease = request.Year,
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                    request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
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
