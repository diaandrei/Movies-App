using Microsoft.EntityFrameworkCore;
using Movies.Application.Database;
using Movies.Application.Models;
using Movies.Contracts.Responses;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MoviesDbContext _dbContext;

    public MovieRepository(MoviesDbContext dbcontext)
    {
        _dbContext = dbcontext;
    }

    public async Task<bool> CreateAsync(Movie movie, List<Genre> genres, List<Cast> casts, IEnumerable<OmdbRatingResponse> ombdRatings, CancellationToken token = default)
    {
        if (_dbContext.Movies.Any(m => m.Title == movie.Title && m.YearOfRelease == movie.YearOfRelease))
        {
            throw new ArgumentException("The movie you are trying to add already exists.");
        }

        foreach (var genre in genres)
        {
            var existingGenre = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Name == genre.Name, token);
            if (existingGenre == null)
            {
                _dbContext.Genres.Add(genre);
            }
            else
            {
                genre.Id = existingGenre.Id;
            }
        }

        foreach (var cast in casts)
        {
            var existingCast = await _dbContext.Casts.FirstOrDefaultAsync(c => c.Name == cast.Name && c.Role == cast.Role, token);
            if (existingCast == null)
            {
                _dbContext.Casts.Add(cast);
            }
            else
            {
                cast.Id = existingCast.Id;
            }
        }

        await _dbContext.Movies.AddAsync(movie, token);
        await _dbContext.SaveChangesAsync(token);

        foreach (var genre in genres)
        {
            var movieGenre = new MovieGenres
            {
                MovieId = movie.Id,
                GenreId = genre.Id
            };
            _dbContext.MovieGenres.Add(movieGenre);
        }

        foreach (var cast in casts)
        {
            var movieCast = new MovieCast
            {
                MovieId = movie.Id,
                CastId = cast.Id
            };
            _dbContext.MovieCast.Add(movieCast);
        }
        foreach (var omdbRating in ombdRatings)
        {
            var rating = new OmdbRating
            {
                Id = Guid.NewGuid(),
                MovieId = movie.Id,
                Source = omdbRating.Source,
                Value = omdbRating.Value
            };
            _dbContext.OmdbRatings.Add(rating);
        }

        await _dbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<Movie> GetByIdAsync(Guid id, bool isAdmin = false, string userId = null, CancellationToken token = default)
    {
        var movie = await _dbContext.Movies
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(token);

        if (movie == null) return null;

        var casts = await _dbContext.MovieCast
            .Where(mc => mc.MovieId == id)
            .Select(mc => new Cast
            {
                Id = mc.CastId,
                Name = mc.Cast.Name,
                Role = mc.Cast.Role
            })
            .ToListAsync(token);

        var genres = await _dbContext.MovieGenres
            .Where(mg => mg.MovieId == id)
            .Select(mg => new Genre
            {
                Id = mg.GenreId,
                Name = mg.Genre.Name
            })
            .ToListAsync(token);

        var externalRatings = await _dbContext.ExternalRatings
            .Where(er => er.MovieId == id)
            .Select(er => new ExternalRating
            {
                Source = er.Source,
                Rating = er.Rating
            })
            .ToListAsync(token);

        var omdbRatings = await _dbContext.OmdbRatings
            .Where(or => or.MovieId == id)
            .Select(or => new OmdbRating
            {
                Source = or.Source,
                Value = or.Value
            })
            .ToListAsync(token);

        var movieRatings = await _dbContext.MovieRatings
            .Where(mr => mr.MovieId == id && mr.UserId == userId).Select(mr => new MovieRating
            {
                Id = mr.Id,
                MovieId = mr.MovieId,
                Rating = mr.Rating,
                UserId = mr.UserId,
                IsUserRated = mr.IsUserRated
            })
            .ToListAsync(token);

        var userWatchlist = await _dbContext.UserWatchlists
            .Where(uw => uw.MovieId == id && uw.UserId == userId).Select(uw => new UserWatchlist
            {
                Id = uw.Id,
                MovieId = uw.MovieId,
                UserId = uw.UserId,
                CreatedAt = uw.CreatedAt,
                UpdatedAt = uw.UpdatedAt,
            })
            .ToListAsync(token);

        ApplicationUser? user = null;
        if (!string.IsNullOrEmpty(userId))
        {
            user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync(token);
        }

        return new Movie
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
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt,
            Cast = casts,
            Genres = genres,
            ExternalRatings = externalRatings,
            OmdbRatings = omdbRatings,
            MovieRatings = movieRatings,
            UserWatchlists = userWatchlist,
            ApplicationUser = user
        };
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, bool isAdmin = false, string userId = null, CancellationToken token = default)
    {
        var query = _dbContext.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(options.Title))
        {
            query = query.Where(x => x.Title.Contains(options.Title));
        }

        if (!string.IsNullOrEmpty(options.YearOfRelease))
        {
            query = query.Where(x => x.YearOfRelease == options.YearOfRelease);
        }

        query = options.SortField switch
        {
            SortField.Title => options.SortOrder == SortOrder.Ascending ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
            _ => query
        };

        var movies = isAdmin
        ? await query.ToListAsync(token) : await query.Skip((options.Page - 1) * options.PageSize).Take(options.PageSize).ToListAsync(token);

        var movieIds = movies.Select(m => m.Id).ToList();

        var casts = await _dbContext.MovieCast
            .Where(mc => movieIds.Contains(mc.MovieId))
            .Include(mc => mc.Cast)
            .ToListAsync(token);

        var genres = await _dbContext.MovieGenres
            .Where(mg => movieIds.Contains(mg.MovieId))
            .Include(mg => mg.Genre)
            .ToListAsync(token);

        var externalRatings = await _dbContext.ExternalRatings
            .Where(er => movieIds.Contains(er.MovieId))
            .ToListAsync(token);

        var omdbRatings = await _dbContext.OmdbRatings
            .Where(or => movieIds.Contains(or.MovieId))
            .ToListAsync(token);

        var movieRatings = await _dbContext.MovieRatings
            .Where(mr => movieIds.Contains(mr.MovieId) && mr.UserId == userId)
            .ToListAsync(token);

        var userWatchlist = await _dbContext.UserWatchlists
            .Where(uw => movieIds.Contains(uw.MovieId) && uw.UserId == userId)
            .ToListAsync(token);

        ApplicationUser? user = null;
        if (!string.IsNullOrEmpty(userId))
        {
            user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync(token);
        }
        var movieDTOs = movies.Select(movie => new Movie
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
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt,

            Cast = casts.Where(mc => mc.MovieId == movie.Id).Select(mc => new Cast
            {
                Id = mc.CastId,
                Name = mc.Cast.Name,
                Role = mc.Cast.Role
            }).ToList(),

            Genres = genres.Where(mg => mg.MovieId == movie.Id).Select(mg => new Genre
            {
                Id = mg.GenreId,
                Name = mg.Genre.Name
            }).ToList(),

            ExternalRatings = externalRatings.Where(er => er.MovieId == movie.Id).Select(er => new ExternalRating
            {
                Source = er.Source,
                Rating = er.Rating
            }).ToList(),

            OmdbRatings = omdbRatings.Where(or => or.MovieId == movie.Id).Select(or => new OmdbRating
            {
                Source = or.Source,
                Value = or.Value
            }).ToList(),

            MovieRatings = movieRatings.Where(mr => mr.MovieId == movie.Id).Select(mr => new MovieRating
            {
                Rating = mr.Rating,
                IsUserRated = mr.IsUserRated,
                Id = mr.Id,
                MovieId = mr.MovieId
            }).ToList(),

            UserWatchlists = userWatchlist.Where(uw => uw.MovieId == movie.Id).Select(uw => new UserWatchlist
            {
                Id = uw.Id,
                MovieId = uw.MovieId,
                UserId = uw.UserId,
                CreatedAt = uw.CreatedAt,
                UpdatedAt = uw.UpdatedAt
            }).ToList(),
            ApplicationUser = user
        }).ToList();

        return movieDTOs;
    }
    public async Task<IEnumerable<Movie>> GetTopMovieAsync(bool isAdmin = false, string userId = null, CancellationToken token = default)
    {
        var topMovieIds = await _dbContext.TopMovies.Take(10)
       .Select(tm => tm.MovieId)
       .ToListAsync(token);

        var query = _dbContext.Movies.Where(m => topMovieIds.Contains(m.Id)).AsQueryable();

        var movies = await query.ToListAsync(token);

        var movieIds = movies.Select(m => m.Id).ToList();

        var casts = await _dbContext.MovieCast
            .Where(mc => movieIds.Contains(mc.MovieId))
            .Include(mc => mc.Cast)
            .ToListAsync(token);

        var genres = await _dbContext.MovieGenres
            .Where(mg => movieIds.Contains(mg.MovieId))
            .Include(mg => mg.Genre)
            .ToListAsync(token);

        var externalRatings = await _dbContext.ExternalRatings
            .Where(er => movieIds.Contains(er.MovieId))
            .ToListAsync(token);

        var omdbRatings = await _dbContext.OmdbRatings
            .Where(or => movieIds.Contains(or.MovieId))
            .ToListAsync(token);

        var movieRatings = await _dbContext.MovieRatings
            .Where(mr => movieIds.Contains(mr.MovieId) && mr.UserId == userId)
            .ToListAsync(token);

        var userWactlist = await _dbContext.UserWatchlists
            .Where(uw => movieIds.Contains(uw.MovieId) && uw.UserId == userId)
            .ToListAsync(token);

        ApplicationUser? user = null;
        if (!string.IsNullOrEmpty(userId))
        {
            user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync(token);
        }

        var movieDTOs = movies.Select(movie => new Movie
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
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt,
            Cast = casts.Where(mc => mc.MovieId == movie.Id).Select(mc => new Cast
            {
                Id = mc.CastId,
                Name = mc.Cast.Name,
                Role = mc.Cast.Role
            }).ToList(),

            Genres = genres.Where(mg => mg.MovieId == movie.Id).Select(mg => new Genre
            {
                Id = mg.GenreId,
                Name = mg.Genre.Name
            }).ToList(),

            ExternalRatings = externalRatings.Where(er => er.MovieId == movie.Id).Select(er => new ExternalRating
            {
                Source = er.Source,
                Rating = er.Rating
            }).ToList(),

            OmdbRatings = omdbRatings.Where(or => or.MovieId == movie.Id).Select(or => new OmdbRating
            {
                Source = or.Source,
                Value = or.Value
            }).ToList(),

            MovieRatings = movieRatings.Where(mr => mr.MovieId == movie.Id).Select(mr => new MovieRating
            {
                Rating = mr.Rating
            }).ToList(),

            UserWatchlists = userWactlist.Where(uw => uw.MovieId == movie.Id).Select(uw => new UserWatchlist
            {
                Id = uw.Id,
                MovieId = uw.MovieId,
                UserId = uw.UserId,
                CreatedAt = uw.CreatedAt,
                UpdatedAt = uw.UpdatedAt,
            }).ToList(),

            ApplicationUser = user
        }).ToList();

        return movieDTOs;
    }

    public async Task<IEnumerable<Movie>> GetMostRecentMovieAsync(bool isAdmin = false, string userId = null, CancellationToken token = default)
    {
        var query = _dbContext.Movies.OrderByDescending(m => m.CreatedAt).Take(10).AsQueryable();

        var movies = await query.ToListAsync(token);

        var movieIds = movies.Select(m => m.Id).ToList();

        var casts = await _dbContext.MovieCast
            .Where(mc => movieIds.Contains(mc.MovieId))
            .Include(mc => mc.Cast)
            .ToListAsync(token);

        var genres = await _dbContext.MovieGenres
            .Where(mg => movieIds.Contains(mg.MovieId))
            .Include(mg => mg.Genre)
            .ToListAsync(token);

        var externalRatings = await _dbContext.ExternalRatings
            .Where(er => movieIds.Contains(er.MovieId))
            .ToListAsync(token);

        var omdbRatings = await _dbContext.OmdbRatings
            .Where(or => movieIds.Contains(or.MovieId))
            .ToListAsync(token);

        var movieRatings = await _dbContext.MovieRatings
                .Where(mr => movieIds.Contains(mr.MovieId) && mr.UserId == userId)
                .ToListAsync(token);
        var userWatchlist = await _dbContext.UserWatchlists
            .Where(uw => movieIds.Contains(uw.MovieId) && uw.UserId == userId)
            .ToListAsync(token);

        ApplicationUser? user = null;
        if (!string.IsNullOrEmpty(userId))
        {
            user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync(token);
        }

        var movieDTOs = movies.Select(movie => new Movie
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
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt,
            Cast = casts.Where(mc => mc.MovieId == movie.Id).Select(mc => new Cast
            {
                Id = mc.CastId,
                Name = mc.Cast.Name,
                Role = mc.Cast.Role
            }).ToList(),

            Genres = genres.Where(mg => mg.MovieId == movie.Id).Select(mg => new Genre
            {
                Id = mg.GenreId,
                Name = mg.Genre.Name
            }).ToList(),

            ExternalRatings = externalRatings.Where(er => er.MovieId == movie.Id).Select(er => new ExternalRating
            {
                Source = er.Source,
                Rating = er.Rating
            }).ToList(),

            OmdbRatings = omdbRatings.Where(or => or.MovieId == movie.Id).Select(or => new OmdbRating
            {
                Source = or.Source,
                Value = or.Value
            }).ToList(),

            MovieRatings = movieRatings.Where(mr => mr.MovieId == movie.Id).Select(mr => new MovieRating
            {
                Rating = mr.Rating
            }).ToList(),

            UserWatchlists = userWatchlist.Where(uw => uw.MovieId == movie.Id).Select(uw => new UserWatchlist
            {
                Id = uw.Id,
                MovieId = uw.MovieId,
                UserId = uw.UserId,
                CreatedAt = uw.CreatedAt,
                UpdatedAt = uw.UpdatedAt,
            }).ToList(),

            ApplicationUser = user
        }).ToList();

        return movieDTOs;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        try
        {
            var existingMovie = await _dbContext.Movies.FindAsync(movie?.Id);

            if (existingMovie == null)
            {
                return false;
            }

            existingMovie.UserRating = movie?.UserRating;
            existingMovie.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(token);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == id, token);
        if (movie == null) return false;

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync(token);

        return true;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == id, token);
        return movie != null;
    }
    public async Task<bool> GetMovieByTitle(string movieTitle, CancellationToken token = default)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Title == movieTitle, token);
        return movie != null;
    }

    public async Task<int> GetCountAsync(string? title, string yearOfRelease, CancellationToken token = default)
    {
        var query = _dbContext.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(x => x.Title.Contains(title));
        }
        if (!string.IsNullOrEmpty(yearOfRelease))
        {
            query = query.Where(x => x.YearOfRelease == yearOfRelease);
        }
        return await query.CountAsync(token);
    }
    public async Task<IEnumerable<Movie>> GetSearchedMoviesAsync(string? textToSearchMovie, CancellationToken token = default)
    {
        var query = _dbContext.Movies.AsQueryable();

        query = query.Where(x => EF.Functions.Like(x.Title, $"%{textToSearchMovie}%") ||
                                EF.Functions.Like(x.YearOfRelease, $"%{textToSearchMovie}%"));

        return await query.ToListAsync(token);
    }

    public async Task<bool> CreateTopMovieAsync(List<TopMovie> movies, CancellationToken token = default)
    {
        if (movies.Count > 0)
        {
            _dbContext.TopMovies.RemoveRange(_dbContext.TopMovies);
            await _dbContext.SaveChangesAsync(token);
            await _dbContext.TopMovies.AddRangeAsync(movies, token);

            await _dbContext.SaveChangesAsync(token);
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<Movie>> GetMoviesByIdsAsync(List<Guid> movieIds, CancellationToken token)
    {
        return await _dbContext.Movies
            .Where(m => movieIds.Contains(m.Id))
            .ToListAsync(token);
    }
}