using AutoMapper;
using EventPulse.BLL.DTOs.Event;
using EventPulse.DAL.Entities;

namespace EventPulse.BLL.Mappings;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<Event, EventResponse>()
            .ForMember(dest => dest.OrganizerName, opt => opt.MapFrom(src => src.Organizer != null ? src.Organizer.Name : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue != null ? src.Venue.Name : null))
            .ForMember(dest => dest.VenueAddress, opt => opt.MapFrom(src => src.Venue != null ? src.Venue.Address : null))
            .ForMember(dest => dest.VenueCity, opt => opt.MapFrom(src => src.Venue != null ? src.Venue.City : null))
            .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.Posters.Select(p => p.PosterUrl).FirstOrDefault()))
            .ForMember(dest => dest.PosterUrls, opt => opt.MapFrom(src => src.Posters.Select(p => p.PosterUrl).ToList()));
    }
}
