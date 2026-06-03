using EventPulse.BLL.DTOs.Booking;

namespace EventPulse.BLL.Interfaces;

public interface ITicketService
{
    Task<List<MyTicketResponse>> GetMyTicketsAsync(int bookingId);
    Task<List<MyTicketResponse>> GetAllMyTicketsAsync();
    Task<(string FullPath, string TicketCode)> GetTicketDownloadInfoAsync(int ticketId);
    Task<CheckInResponse> CheckInAsync(string ticketCode);
}
