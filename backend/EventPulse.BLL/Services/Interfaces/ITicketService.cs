using EventPulse.BLL.DTOs.Booking;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.BLL.Interfaces;

public interface ITicketService
{
    Task<List<MyTicketResponse>> GetMyTicketsAsync(int bookingId);
    Task<PagedResult<MyTicketResponse>> GetAllMyTicketsAsync(PageRequest pageRequest);
    Task<(string FullPath, string TicketCode)> GetTicketDownloadInfoAsync(int ticketId);
    Task<CheckInResponse> CheckInAsync(string ticketCode);
}
