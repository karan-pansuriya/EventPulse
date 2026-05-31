using AutoMapper;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.DAL.Entities;

namespace EventPulse.BLL.Mappings;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        // TicketDto — PdfUrl and QrCodeUrl are raw paths here.
        // Full base URL is prepended in the controller where HttpContext is available.
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.QrCodeUrl, opt => opt.MapFrom(src => src.QrCodePath))
            .ForMember(dest => dest.PdfUrl,    opt => opt.MapFrom(src => src.PdfPath));

        CreateMap<Booking, BookingResponse>()
            .ForMember(dest => dest.EventTitle,
                opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : null))
            .ForMember(dest => dest.TicketCodes,
                opt => opt.MapFrom(src => src.Tickets.Select(t => t.TicketCode).ToList()))
            .ForMember(dest => dest.Tickets,
                opt => opt.MapFrom(src => src.Tickets));
    }
}