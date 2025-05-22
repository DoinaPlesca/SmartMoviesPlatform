using AutoMapper;
using MongoDB.Bson;
using WatchlistService.Application.Dtos;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Infrastructure.Mappings;

public class WatchlistMappingProfile : Profile
{
    public WatchlistMappingProfile()
    {
        CreateMap<MovieItem, MovieItemDto>()
            .ForMember(dest => dest.ExtraFields, opt =>
                opt.MapFrom(src => src.ExtraElements.Elements
                    .ToDictionary(e => e.Name, e => BsonTypeMapper.MapToDotNetValue(e.Value))
                ));
        CreateMap<Watchlist, WatchlistDto>();
    }
}