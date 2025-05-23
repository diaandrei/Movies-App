﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Movies.Application.Database;
using Movies.Application.Models;
using Movies.Application.Services;

namespace Movies.Application.Validators
{
    public class MovieValidator : AbstractValidator<Movie>
    {
        private readonly MoviesDbContext _dbContext;
        private readonly IOmdbService _omdbService;

        public MovieValidator(MoviesDbContext dbContext, IOmdbService omdbService)
        {
            _dbContext = dbContext;
            _omdbService = omdbService;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Title id cannot be empty.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title cannot be empty.");

            RuleFor(x => x.YearOfRelease)
                .NotEmpty()
                .WithMessage("Year Of Release cannot be empty.");

            RuleFor(x => x)
                .MustAsync(NotBeDuplicateMovieAsync)
                .WithMessage("This title already exists.");

            RuleFor(x => x.Title)
                .MustAsync(BeAValidMovieAsync)
                .WithMessage("The title does not exist.");
        }

        private async Task<bool> NotBeDuplicateMovieAsync(Movie movie, CancellationToken token)
        {
            try
            {
                var existingMovie = await _dbContext.Movies
                    .FirstOrDefaultAsync(m => m.Title == movie.Title && m.YearOfRelease == movie.YearOfRelease, token);
                return existingMovie == null || existingMovie.Id == movie.Id;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> BeAValidMovieAsync(string title, CancellationToken token)
        {
            var omdbResponse = await _omdbService.GetMovieAsync(title, string.Empty, token);
            return omdbResponse != null && !string.IsNullOrEmpty(omdbResponse.Title);
        }
    }
}
