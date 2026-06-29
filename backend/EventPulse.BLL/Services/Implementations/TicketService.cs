using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EventPulse.BLL.Services;

public class TicketService : BaseService, ITicketService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;
    public TicketService(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment environment)
        : base(httpContextAccessor)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<List<MyTicketResponse>> GetMyTicketsAsync(int bookingId)
    {
        int userId = GetUserId();
        return await _bookingRepository.GetUserBookingsAsync(userId, bookingId);

    }

    public async Task<PagedResult<MyTicketResponse>> GetAllMyTicketsAsync(PageRequest pageRequest)
    {
        int userId = GetUserId();
        return await _bookingRepository.GetPagedUserBookingsAsync(userId, pageRequest);

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

        string fullPath = Path.Combine(_environment.WebRootPath, ticket.PdfPath);
        if (!File.Exists(fullPath))
            throw new NotFoundException("PDF file not found on server.");

        return (fullPath, ticket.TicketCode);
    }

    public async Task<CheckInResponse> CheckInAsync(string ticketCode)
    {
        TicketCheckInProjection? ticketInfo = await _bookingRepository.GetTicketByCodeAsync(ticketCode);
        if (ticketInfo == null)
            throw new NotFoundException("Ticket not found.");

        int userId = GetUserId();
        if (ticketInfo.EventOrganizerId != userId)
            throw new ForbiddenException("This ticket does not belong to an event you manage.");

        if (ticketInfo.IsUsed)
            throw new BadRequestException("This ticket has already been checked in.");

        await _bookingRepository.MarkTicketAsUsedAsync(ticketInfo.TicketId);
        await _unitOfWork.SaveAsync();

        return new CheckInResponse
        {
            Success = true,
            Message = "Check-in successful.",
            TicketId = ticketInfo.TicketId,
            TicketCode = ticketInfo.TicketCode,
            EventTitle = ticketInfo.EventTitle,
            AttendeeName = ticketInfo.AttendeeName,
            CheckedInAt = DateTime.UtcNow,
        };
    }
}
