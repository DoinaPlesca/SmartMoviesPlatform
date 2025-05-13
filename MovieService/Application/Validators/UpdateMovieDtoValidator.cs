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

        RuleFor(x => x.PosterFile!.Length)
            .GreaterThan(0).WithMessage("Poster file cannot be empty.");

        RuleFor(x => x.PosterFile!.ContentType)
            .Must(type => type == "image/jpeg" || type == "image/png")
            .WithMessage("Poster must be a JPEG or PNG.");
        
        RuleFor(x => x.VideoFile!.Length)
            .GreaterThan(0).WithMessage("Video file cannot be empty.");

        RuleFor(x => x.VideoFile!.ContentType)
            .Must(type => type == "video/mp4")
            .WithMessage("Video must be an MP4 file.");
        
    }
}