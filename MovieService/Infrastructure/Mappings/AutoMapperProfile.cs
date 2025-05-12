using AutoMapper;
using MovieService.Application.DTOs;
using MovieService.Domain.Entities;

namespace MovieService.Infrastructure.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UpdateMovieDto, Movie>();
        CreateMap<Movie, MovieDto>()
            .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre.Name));

        CreateMap<CreateMovieDto, Movie>();
        CreateMap<Genre, GenreDto>();

    }
}