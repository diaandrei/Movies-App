using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators
{
    public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
    {
        private static readonly List<string> AcceptableSortFields = new()
        {
            "title",
            "yearofrelease"
        };

        public GetAllMoviesOptionsValidator()
        {
            RuleFor(x => x.SortField)
                .Must(x => x is null || AcceptableSortFields.Contains(x.Value.ToString()))
                .WithMessage("You can only sort by title or year of release");

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be greater than or equal to 1");
        }
    }
}
