using FluentValidation;
using MovieService.Application.DTOs;

namespace MovieService.Application.Validators;

public class UpdateMovieDtoValidator : AbstractValidator<UpdateMovieDto>
{
    public UpdateMovieDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Movie ID is required.")
            .WithErrorCode("ID_ERROR");
        
        RuleFor(x => x.Title)
            .NotEmpty().MaximumLength(100).WithMessage("Title must be less than 100 characters.")
            .WithErrorCode("TITLE_ERROR");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must be less than 1000 characters.")
            .WithErrorCode("DESCRIPTION_ERROR");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10).NotEmpty().WithMessage("Rating must be between 0 and 10.")
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