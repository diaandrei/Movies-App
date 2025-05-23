﻿using Microsoft.EntityFrameworkCore;
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
        _dbContext.Entry(movie).State = EntityState.Detached;

        if (_dbContext.Movies.Any(m => m.Title == movie.Title && m.YearOfRelease == movie.YearOfRelease))
        {
            throw new ArgumentException("The title you are trying to add already exists.");
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

        var yearOfRelease = movie.YearOfRelease.EndsWith("-")
            ? movie.YearOfRelease = movie.YearOfRelease.Replace("-", "")
            : movie.YearOfRelease;
        movie.YearOfRelease = yearOfRelease;

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
            var ratingValue = omdbRating.Value;
            if (!string.IsNullOrEmpty(ratingValue) && ratingValue.Contains("/"))
            {
                var parts = ratingValue.Split('/');

                if (double.TryParse(parts[0], out var numerator) && double.TryParse(parts[1], out var denominator) && denominator != 0)
                {

                    double percentage = (numerator / denominator) * 100;
                    string roundedPercentage = Math.Round(percentage, 2).ToString("0");

                    ratingValue = roundedPercentage + "%";
                }
            }
            var rating = new OmdbRating
            {
                Id = Guid.NewGuid(),
                MovieId = movie.Id,
                Source = omdbRating.Source,
                Value = ratingValue
            };
            _dbContext.OmdbRatings.Add(rating);
        }

        await _dbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<Movie> GetByIdAsync(Guid id, bool isAdmin = false, string userId = null!, CancellationToken token = default)
    {
        var movie = await _dbContext.Movies
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(token);

        if (movie == null) return null!;

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
                Id = or.Id,
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

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, bool isAdmin = false, string userId = null!, CancellationToken token = default)
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
                Id = or.Id,
                Source = or.Source,
                Value = or.Value
            }).ToList(),
            MovieRatings = movieRatings.Where(mr => mr.MovieId == movie.Id).Select(mr => new MovieRating
            {
                UserId = mr.UserId,
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
    public async Task<IEnumerable<Movie>> GetTopMovieAsync(bool isAdmin = false, string userId = null!, CancellationToken token = default)
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
                Id = mr.Id,
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

    public async Task<IEnumerable<Movie>> GetMostRecentMovieAsync(bool isAdmin = false, string userId = null!, CancellationToken token = default)
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
                Id = mr.Id,
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
            var existingMovie = await GetByIdAsync(movie.Id, token: token);

            if (existingMovie == null)
            {
                return false;
            }

            await UpdateMovieBasicProperties(existingMovie, movie, token);
            await UpdateOrAddCastAsync(existingMovie, movie.Cast, token);
            await UpdateOrAddGenresAsync(existingMovie, movie.Genres, token);
            await UpdateOrAddOmdbRatingsAsync(existingMovie, movie.OmdbRatings, token);
            await UpdateOrAddMovieRatingsAsync(existingMovie, movie.MovieRatings, token);

            await _dbContext.SaveChangesAsync(token);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private async Task UpdateMovieBasicProperties(Movie existingMovie, Movie movie, CancellationToken token)
    {
        var db = await _dbContext.Movies.FindAsync(movie?.Id);
        db.Title = !string.IsNullOrEmpty(movie!.Title) ? movie.Title : existingMovie.Title;
        db.Released = !string.IsNullOrEmpty(movie.Released) ? movie.Released : existingMovie.Released;
        db.Runtime = !string.IsNullOrEmpty(movie.Runtime) ? movie.Runtime : existingMovie.Runtime;
        db.YearOfRelease = !string.IsNullOrEmpty(movie.YearOfRelease) ? movie.YearOfRelease : existingMovie.YearOfRelease;
        db.Rated = !string.IsNullOrEmpty(movie.Rated) ? movie.Rated : existingMovie.Rated;
        db.Plot = !string.IsNullOrEmpty(movie.Plot) ? movie.Plot : existingMovie.Plot;
        db.Awards = !string.IsNullOrEmpty(movie.Awards) ? movie.Awards : existingMovie.Awards;
        db.Poster = !string.IsNullOrEmpty(movie.Poster) ? movie.Poster : existingMovie.Poster;
        db.TotalSeasons = !string.IsNullOrEmpty(movie.TotalSeasons) ? movie.TotalSeasons : existingMovie.TotalSeasons;
        db.IsActive = movie.IsActive;
        db.Rating = movie.Rating.HasValue ? movie.Rating : existingMovie.Rating;
        db.UserRating = movie.UserRating.HasValue ? movie.UserRating : existingMovie.UserRating;
        db.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(token);
    }

    private async Task UpdateOrAddCastAsync(Movie existingMovie, List<Cast> newCasts, CancellationToken token)
    {
        var existingCasts = await _dbContext.Casts.Where(c => newCasts.Select(ec => ec.Name).Contains(c.Name)).ToListAsync(token);
        var castIds = new List<Guid>();
        var newCastId = Guid.NewGuid();
        foreach (var newCast in newCasts)
        {
            var existingCast = existingCasts.FirstOrDefault(c => c.Name == newCast.Name);

            if (existingCast != null)
            {
                existingCast.Name = newCast.Name;
                existingCast.Role = newCast.Role;
                castIds.Add(existingCast.Id);
            }
            else
            {
                var castToAdd = new Cast
                {
                    Id = newCastId,
                    Name = newCast.Name,
                    Role = newCast.Role,
                };
                _dbContext.Casts.Add(castToAdd);
                castIds.Add(newCastId);

            }
        }
        await _dbContext.SaveChangesAsync(token);

        var existingMovieCasts = await _dbContext.MovieCast
            .Where(mc => mc.MovieId == existingMovie.Id)
            .ToListAsync(token);

        _dbContext.MovieCast.RemoveRange(existingMovieCasts);

        foreach (var castId in castIds)
        {
            var movieCast = new MovieCast
            {
                MovieId = existingMovie.Id,
                CastId = castId
            };
            _dbContext.MovieCast.Add(movieCast);
        }
        await _dbContext.SaveChangesAsync(token);
    }

    private async Task UpdateOrAddGenresAsync(Movie existingMovie, List<Genre> newGenres, CancellationToken token)
    {
        var existingGenres = await _dbContext.Genres.Where(g => newGenres.Select(ng => ng.Name).Contains(g.Name)).ToListAsync(token);
        var genreIds = new List<Guid>();
        var newId = Guid.NewGuid();

        foreach (var newGenre in newGenres)
        {
            var existingGenre = existingGenres.FirstOrDefault(g => g.Name == newGenre.Name);

            if (existingGenre != null)
            {
                existingGenre.Name = newGenre.Name;
                genreIds.Add(existingGenre.Id);
            }
            else
            {
                var genreToAdd = new Genre
                {
                    Id = newId,
                    Name = newGenre.Name,
                    UpdatedAt = newGenre.UpdatedAt
                };
                _dbContext.Genres.Add(genreToAdd);
                genreIds.Add(newId);
            }
        }
        await _dbContext.SaveChangesAsync(token);

        var existingMovieGenre = await _dbContext.MovieGenres
            .Where(mc => mc.MovieId == existingMovie.Id)
            .ToListAsync(token);

        _dbContext.MovieGenres.RemoveRange(existingMovieGenre);

        foreach (var genreId in genreIds)
        {
            var movieGenre = new MovieGenres
            {
                MovieId = existingMovie.Id,
                GenreId = genreId
            };
            _dbContext.MovieGenres.Add(movieGenre);
        }
        await _dbContext.SaveChangesAsync(token);
    }

    private async Task UpdateOrAddOmdbRatingsAsync(Movie existingMovie, List<OmdbRating> newOmdbRatings, CancellationToken token)
    {
        var existingOmdbRatings = await _dbContext.OmdbRatings.Where(or => or.MovieId == existingMovie.Id).ToListAsync(token);

        var newId = Guid.NewGuid();

        foreach (var newOmdbRating in newOmdbRatings)
        {
            var existingOmdbRating = existingOmdbRatings.FirstOrDefault(or => or.Source == newOmdbRating.Source);

            if (existingOmdbRating != null)
            {
                existingOmdbRating.Value = newOmdbRating.Value;
            }
            else
            {
                var ombRatingToAdd = new OmdbRating
                {
                    Id = newId,
                    Source = newOmdbRating.Source,
                    MovieId = existingMovie.Id,
                    Value = newOmdbRating.Value
                };

                _dbContext.OmdbRatings.Add(ombRatingToAdd);
            }
        }
        await _dbContext.SaveChangesAsync(token);
    }

    private async Task UpdateOrAddMovieRatingsAsync(Movie existingMovie, List<MovieRating> newMovieRatings, CancellationToken token)
    {
        var existingMovieRatings = await _dbContext.MovieRatings.Where(mr => mr.MovieId == existingMovie.Id).ToListAsync(token);

        var newId = Guid.NewGuid();

        foreach (var newMovieRating in newMovieRatings)
        {
            var existingMovieRating = existingMovieRatings.FirstOrDefault(mr => mr.UserId == newMovieRating.UserId);
            if (existingMovieRating != null)
            {
                existingMovieRating.Rating = newMovieRating.Rating;
            }
            else
            {
                var movieRatingToAdd = new MovieRating
                {

                    UserId = newMovieRating.UserId,
                    Rating = newMovieRating.Rating,
                    IsUserRated = newMovieRating.IsUserRated,
                    UpdatedAt = newMovieRating.UpdatedAt
                };
                _dbContext.MovieRatings.Add(movieRatingToAdd);
            }
        }
        await _dbContext.SaveChangesAsync(token);
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
        if (string.IsNullOrWhiteSpace(textToSearchMovie))
        {
            return Enumerable.Empty<Movie>();
        }

        var query = _dbContext.Movies.AsQueryable();

        query = query.Where(x => EF.Functions.Like(x.Title, $"%{textToSearchMovie}%") ||
                                 EF.Functions.Like(x.YearOfRelease, $"%{textToSearchMovie}%"));
        query = query.OrderBy(x => EF.Functions.Like(x.Title, $"{textToSearchMovie}%") ? 0 : 1);

        query = query.Take(5);

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