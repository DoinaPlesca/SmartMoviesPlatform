using AutoMapper;
using WatchlistService.Application.Dtos;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Infrastructure.Mappings;

public class WatchlistMappingProfile : Profile
{
    public WatchlistMappingProfile()
    {
        CreateMap<MovieItem, MovieItemDto>();
        CreateMap<Watchlist, WatchlistDto>();
    }
}