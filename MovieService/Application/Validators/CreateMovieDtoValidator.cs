using FluentValidation;
using MovieService.Application.DTOs;

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
        
        RuleFor(x => x.VideoUrl)
            .NotEmpty().WithMessage("Video URL is required.")
            .WithErrorCode("VIDEO_URL_ERROR");

        RuleFor(x => x.PosterUrl)
            .NotEmpty().WithMessage("Poster URL is required.")
            .WithErrorCode("POSTER_URL_ERROR");
    }
}