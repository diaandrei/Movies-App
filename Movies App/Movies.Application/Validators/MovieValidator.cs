using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application.Validators
{
    public class MovieValidator : AbstractValidator<Movie>
    {
        private readonly IMovieRepository _movieRepository;
        public MovieValidator(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
            RuleFor(x => x.Id)
                .NotEmpty();
            RuleFor(x => x.Title)
                .NotEmpty();
            RuleFor(x => x.YearOfRelease)
                .LessThanOrEqualTo(DateTime.UtcNow.Year);
        }
    }
}
