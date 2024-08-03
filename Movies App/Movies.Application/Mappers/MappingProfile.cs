using AutoMapper;
using Movies.Application.DataTransferObjects;
using Movies.Application.Models;

namespace Movies.Application.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movie, MovieDto>()
                .ForMember(dest => dest.UserWatchlistId, opt => opt.Ignore());

            CreateMap<Cast, CastDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));

            CreateMap<Genre, GenreDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<ExternalRating, ExternalRatingDto>();

            CreateMap<OmdbRating, OmdbRatingDto>();

            CreateMap<MovieRating, MovieRatingDto>();
        }
    }
}