using FluentValidation;
using MovieService.Application.Dtos.Movie;

namespace MovieService.Application.Validators;

public class CreateMovieDtoValidator : AbstractValidator<CreateMovieDto>
{
    public CreateMovieDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must be less than 100 characters.")
            .WithErrorCode("TITLE_ERROR");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must be less than 1000 characters.")
            .WithErrorCode("DESCRIPTION_ERROR");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10).WithMessage("Rating must be between 0 and 10.")
            .WithErrorCode("RATING_ERROR");

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Release date must be less than today's date.")
            .WithErrorCode("RELEASE_DATE_ERROR");
        
        RuleFor(x => x.VideoFile)
            .NotNull().WithMessage("Video file is required.")
            .Must(f => f.Length > 0).WithMessage("Video file cannot be empty.")
            .WithErrorCode("VIDEO_FILE_ERROR");

        RuleFor(x => x.PosterFile)
            .NotNull().WithMessage("Poster image is required.")
            .Must(f => f.Length > 0).WithMessage("Poster file cannot be empty.")
            .WithErrorCode("POSTER_FILE_ERROR");
    }
}