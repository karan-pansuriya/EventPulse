namespace EventPulse.BLL.DTOs.Booking;

public class CheckInResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TicketId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string EventTitle { get; set; } = string.Empty;
    public string? AttendeeName { get; set; }
    public DateTime CheckedInAt { get; set; }
}
