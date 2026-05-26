using AutoMapper;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.DAL.Entities;

namespace EventPulse.BLL.Mappings;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<Booking, BookingResponse>()
            .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : null))
            .ForMember(dest => dest.TicketCodes, opt => opt.MapFrom(src => src.Tickets.Select(t => t.TicketCode).ToList()));
    }
}
