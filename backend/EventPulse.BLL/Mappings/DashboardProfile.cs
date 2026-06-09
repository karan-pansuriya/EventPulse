using AutoMapper;
using EventPulse.BLL.DTOs.Dashboard;
using EventPulse.BLL.DTOs.Event;
using EventPulse.DAL.Entities;

namespace EventPulse.BLL.Mappings;

public class DashboardProfile : Profile
{
    public DashboardProfile()
    {
        CreateMap<Event, TopBookedEventDto>()
            // .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.Posters.Select(p => p.PosterUrl).FirstOrDefault()))
            .ForMember(dest => dest.OrganizerName, opt => opt.MapFrom(src => src.Organizer != null ? src.Organizer.Name : "Unknown"))
            .ForMember(dest => dest.TotalBookings, opt => opt.Ignore())
            .ForMember(dest => dest.RevenueGenerated, opt => opt.Ignore());

        CreateMap<Booking, EventAttendeeDto>()
            .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : null))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
            .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.User != null ? src.User.Phone : null))
            .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.EventId))
            .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : null))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
            .ForMember(dest => dest.BookedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}