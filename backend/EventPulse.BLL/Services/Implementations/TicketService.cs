using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventPulse.BLL.Services;

public class TicketService : BaseService, ITicketService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly string _webRootPath;

    public TicketService(
        IBookingRepository bookingRepository,
        IHttpContextAccessor httpContextAccessor,
        string webRootPath)
        : base(httpContextAccessor)
    {
        _bookingRepository = bookingRepository;
        _webRootPath = webRootPath;
    }

    public async Task<List<MyTicketResponse>> GetMyTicketsAsync(int bookingId)
    {
        int userId = GetUserId();
        List<Booking> bookings = await _bookingRepository.GetUserBookingsAsync(userId, bookingId);

        HttpRequest? request = HttpContextAccessor.HttpContext?.Request;
        string baseUrl = request != null
            ? $"{request.Scheme}://{request.Host}"
            : string.Empty;

        return bookings.Select(b => new MyTicketResponse
        {
            BookingId = b.Id,
            BookingCode = b.UniqueCode,
            EventTitle = b.Event?.Title ?? string.Empty,
            EventDate = b.Event != null ? DateOnly.FromDateTime(b.Event.EventDate) : null,
            VenueName = b.Event?.Venue?.Name,
            VenueCity = b.Event?.Venue?.City?.Name,
            Quantity = b.Quantity,
            TotalAmount = b.TotalAmount,
            CreatedAt = b.CreatedAt,
            Tickets = b.Tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                TicketCode = t.TicketCode,
                QrCodeUrl = t.QrCodePath != null ? $"{baseUrl}/{t.QrCodePath}" : null,
                PdfUrl = t.PdfPath != null ? $"{baseUrl}/{t.PdfPath}" : null,
                IsUsed = t.IsUsed,
                UsedAt = t.UsedAt,
            }).ToList()
        }).ToList();
    }

    public async Task<List<MyTicketResponse>> GetAllMyTicketsAsync()
    {
        int userId = GetUserId();
        List<Booking> bookings = await _bookingRepository.GetUserAllBookingsAsync(userId);

        HttpRequest? request = HttpContextAccessor.HttpContext?.Request;
        string baseUrl = request != null
            ? $"{request.Scheme}://{request.Host}"
            : string.Empty;

        return bookings.Select(b => new MyTicketResponse
        {
            BookingId = b.Id,
            BookingCode = b.UniqueCode,
            EventTitle = b.Event?.Title ?? string.Empty,
            EventDate = b.Event != null ? DateOnly.FromDateTime(b.Event.EventDate) : null,
            VenueName = b.Event?.Venue?.Name,
            VenueCity = b.Event?.Venue?.City?.Name,
            Quantity = b.Quantity,
            TotalAmount = b.TotalAmount,
            CreatedAt = b.CreatedAt,
            Tickets = b.Tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                TicketCode = t.TicketCode,
                QrCodeUrl = t.QrCodePath != null ? $"{baseUrl}/{t.QrCodePath}" : null,
                PdfUrl = t.PdfPath != null ? $"{baseUrl}/{t.PdfPath}" : null,
                IsUsed = t.IsUsed,
                UsedAt = t.UsedAt,
            }).ToList()
        }).ToList();
    }

    public async Task<(string FullPath, string TicketCode)> GetTicketDownloadInfoAsync(int ticketId)
    {
        int userId = GetUserId();
        Booking? booking = await _bookingRepository.GetBookingByTicketIdAsync(ticketId);
        if (booking == null || booking.UserId != userId)
            throw new NotFoundException("Ticket not found.");

        Ticket? ticket = booking.Tickets.FirstOrDefault(t => t.Id == ticketId);
        if (ticket?.PdfPath == null)
            throw new NotFoundException("PDF not available for this ticket.");

        string fullPath = Path.Combine(_webRootPath, ticket.PdfPath);
        if (!File.Exists(fullPath))
            throw new NotFoundException("PDF file not found on server.");

        return (fullPath, ticket.TicketCode);
    }

    public async Task<CheckInResponse> CheckInAsync(string ticketCode)
    {
        Ticket? ticket = await _bookingRepository.GetTicketByCodeAsync(ticketCode);
        if (ticket == null)
            throw new NotFoundException("Ticket not found.");

        Booking? booking = ticket.Booking;
        Event? eventEntity = booking?.Event;

        if (eventEntity == null)
            throw new BadRequestException("Ticket is not associated with a valid event.");

        int userId = GetUserId();
        if (eventEntity.OrganizerId != userId)
            throw new ForbiddenException("This ticket does not belong to an event you manage.");

        if (ticket.IsUsed)
            throw new BadRequestException("This ticket has already been checked in.");

        ticket.IsUsed = true;
        ticket.UsedAt = DateTime.UtcNow;

        await _bookingRepository.MarkTicketAsUsedAsync(ticket);

        return new CheckInResponse
        {
            Success = true,
            Message = "Check-in successful.",
            TicketId = ticket.Id,
            TicketCode = ticket.TicketCode,
            EventTitle = eventEntity.Title,
            AttendeeName = booking?.User?.Name ?? booking?.User?.Email,
            CheckedInAt = ticket.UsedAt.Value,
        };
    }
}
